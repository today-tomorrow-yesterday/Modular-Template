using Asp.Versioning;
using Modules.Customer.Infrastructure;
using Modules.Customer.Infrastructure.Persistence;
using Modules.Customer.Presentation.Endpoints;
using Rtl.Core.Api.Shared;
using Rtl.Core.Application;
using Rtl.Core.Infrastructure;
using Rtl.Core.Infrastructure.Secrets;
using CustomerApplication = Modules.Customer.Application.AssemblyReference;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// Configuration
// ========================================

var databaseConnectionString = DatabaseConnectionResolver.Resolve(builder.Configuration);

var cacheConnectionString = builder.Configuration.GetConnectionString("Cache")
    ?? "localhost:6379";

// ========================================
// Service Configuration
// ========================================

var moduleEndpoints = new CustomerModuleEndpoints();

// Add shared host defaults (health checks, versioning, OpenAPI, exception handling)
builder.AddModuleHostDefaults(
    "Customer",
    databaseConnectionString,
    cacheConnectionString,
    moduleEndpoints);

// Application layer (MediatR, FluentValidation, Pipeline Behaviors)
builder.Services.AddCommonApplication([CustomerApplication.Assembly]);

// Infrastructure layer (Database, Cache, Auth, Workers, Messaging)
builder.Services.AddCommonInfrastructure(
    builder.Configuration,
    builder.Environment,
    databaseConnectionString,
    cacheConnectionString);

// Customer module services
builder.Services.AddCustomerModule(
    builder.Configuration,
    builder.Environment,
    databaseConnectionString);

// ========================================
// Middleware Pipeline
// ========================================

var app = builder.Build();

// Apply migrations in development
app.ApplyMigrations<CustomerDbContext>(builder.Environment, databaseConnectionString);

// Create the API version set for endpoint mapping
var apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1, 0))
    .HasApiVersion(new ApiVersion(2, 0))
    .ReportApiVersions()
    .Build();

// Apply shared middleware
app.UseModuleHostDefaults(moduleEndpoints);

// Map module endpoints
moduleEndpoints.MapEndpoints(app, apiVersionSet);

app.Run();

// Expose Program class for integration tests
public partial class Program;
