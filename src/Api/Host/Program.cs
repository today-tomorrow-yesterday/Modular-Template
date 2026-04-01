using Modules.SampleOrders.Infrastructure;
using Modules.SampleOrders.Infrastructure.Persistence;
using Modules.SampleSales.Infrastructure;
using Modules.SampleSales.Infrastructure.Persistence;
using ModularTemplate.Api.Extensions;
using ModularTemplate.Api.Shared;
using ModularTemplate.Application;
using ModularTemplate.Infrastructure;
using ModularTemplate.Infrastructure.Application;
using ModularTemplate.Infrastructure.Secrets;
using Serilog;
using OrdersApplication = Modules.SampleOrders.Application.AssemblyReference;
using SampleApplication = Modules.SampleSales.Application.AssemblyReference;

var builder = WebApplication.CreateBuilder(args);
var modules = ModuleExtensions.GetModuleEndpoints();

// ========================================
// Host Configuration
// ========================================

// Configure graceful shutdown timeout for chaos engineering readiness
// Allows time for in-flight requests to complete before forced termination
builder.Host.ConfigureHostOptions(options =>
{
    options.ShutdownTimeout = TimeSpan.FromSeconds(30);
});

// ========================================
// Serilog Configuration
// ========================================
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// ========================================
// Service Configuration
// ========================================

var databaseConnectionString = DatabaseConnectionResolver.Resolve(builder.Configuration);

// Fail-Fast: Enforce SSL in Production
if (builder.Environment.IsProduction() && 
    !databaseConnectionString.Contains("SSL Mode=Require", StringComparison.OrdinalIgnoreCase) &&
    !databaseConnectionString.Contains("SSL Mode=VerifyFull", StringComparison.OrdinalIgnoreCase))
{
    // Sanitize connection string for logging (hide password)
    var sanitizedString = databaseConnectionString.Replace(
        builder.Configuration["ConnectionStrings:Database"]!.Split(';').First(s => s.Contains("Password", StringComparison.OrdinalIgnoreCase)), 
        "Password=***");
        
    throw new InvalidOperationException(
        "FATAL: Production database connections MUST use 'SSL Mode=Require' or 'VerifyFull' for FTC compliance. " +
        $"Current connection string: {sanitizedString}");
}

var cacheConnectionString = builder.Configuration.GetConnectionString("Cache")
    ?? "localhost:6379";

// Load module-specific configuration files
builder.Configuration.AddModuleConfiguration(["SampleSales", "SampleOrders"], builder.Environment.EnvironmentName);

// ========================================
// Common Cross-Cutting Concerns
// ========================================

// Application identity - must be registered first as other services depend on it
builder.Services.AddOptions<ApplicationOptions>()
    .Bind(builder.Configuration.GetSection(ApplicationOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Presentation/API layer
builder.Services
    .AddGlobalExceptionHandling()
    .AddApiVersioningServices()
    .AddOpenApiVersioned(builder.Configuration["Application:DisplayName"] ?? "API", modules)
    .AddCorsServices(builder.Configuration, builder.Environment)
    .AddHealthChecks(databaseConnectionString, cacheConnectionString)
    .AddGranularHealthChecks(builder.Configuration)
    .AddRateLimiting(builder.Configuration);

// Application layer (MediatR, FluentValidation, Pipeline Behaviors)
builder.Services.AddCommonApplication([
    SampleApplication.Assembly,
    OrdersApplication.Assembly]);

// Infrastructure layer (Database, Cache, Auth, Workers, Messaging)
builder.Services.AddCommonInfrastructure(
    builder.Configuration,
    builder.Environment,
    databaseConnectionString,
    cacheConnectionString);

// ========================================
// Module Registrations
// ========================================

builder.Services
    .AddSampleSalesModule(builder.Configuration, builder.Environment, DatabaseMigrationExtensions.GetModuleConnectionString(builder.Configuration, "SampleSales", databaseConnectionString))
    .AddSampleOrdersModule(builder.Configuration, builder.Environment, DatabaseMigrationExtensions.GetModuleConnectionString(builder.Configuration, "SampleOrders", databaseConnectionString));

// ========================================
// Middleware Pipeline
// ========================================

var app = builder.Build();

// Apply migrations for all modules (supports multi-database when configured)
app.ApplyMigrations(
    builder.Environment,
    builder.Configuration,
    databaseConnectionString,
    ("SampleSales", typeof(SampleDbContext)),
    ("SampleOrders", typeof(OrdersDbContext)));

// Seed data after migrations (controlled via Seeding:Enabled in appsettings)
await app.SeedDataAsync(builder.Configuration);

// Create the API version set for endpoint mapping
var apiVersionSet = app.CreateApiVersionSet();

// Serilog request logging
app.UseSerilogRequestLogging();

app.UseOpenApiVersioned(modules);

// Health check endpoints for Kubernetes probes and monitoring
app.MapHealthCheckEndpoint();                                      // /health - full health check
app.MapLivenessProbeEndpoint();                                    // /health/live - minimal, just checks app responds
app.MapReadinessProbeEndpoint();                                   // /health/ready - database + cache connectivity
app.MapStartupProbeEndpoint();                                     // /health/startup - database only
app.MapTaggedHealthCheckEndpoint("/health/messaging", "messaging");
app.MapTaggedHealthCheckEndpoint("/health/modules", "module");

app.UseGlobalExceptionHandling();
app.UseRateLimiter();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Map versioned module endpoints
app.MapVersionedModuleEndpoints(apiVersionSet, modules);

app.Run();

// Expose Program class for integration tests
public partial class Program;
