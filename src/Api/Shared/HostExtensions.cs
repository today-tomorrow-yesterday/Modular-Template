using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModularTemplate.Infrastructure.Application;
using ModularTemplate.Presentation.Endpoints;
using Serilog;

namespace ModularTemplate.Api.Shared;

/// <summary>
/// Extension methods for configuring per-module API hosts with common defaults.
/// Use these methods in module-specific host projects for consistent configuration.
/// </summary>
public static class HostExtensions
{
    public static WebApplicationBuilder AddModuleHostDefaults(
        this WebApplicationBuilder builder,
        string moduleName,
        string databaseConnectionString,
        string? cacheConnectionString = null,
        IModuleEndpoints? moduleEndpoints = null)
    {
        // Serilog
        builder.Host.UseSerilog((context, configuration) =>
            configuration.ReadFrom.Configuration(context.Configuration));

        // Fail-Fast: Enforce SSL in Production
        EnforceSslInProduction(builder, databaseConnectionString);

        // Application identity - must be registered first as other services depend on it
        builder.Services.AddOptions<ApplicationOptions>()
            .Bind(builder.Configuration.GetSection(ApplicationOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Health checks
        var healthChecks = builder.Services.AddHealthChecks()
            .AddNpgSql(databaseConnectionString, name: "database");

        if (!string.IsNullOrEmpty(cacheConnectionString))
        {
            healthChecks.AddRedis(cacheConnectionString, name: "cache");
        }

        // API versioning
        builder.Services.AddApiVersioningServices();

        // OpenAPI/Swagger
        if (moduleEndpoints != null)
        {
            builder.Services.AddOpenApiForModule(moduleName, moduleEndpoints);
        }
        else
        {
            builder.Services.AddOpenApiSimple(moduleName);
        }

        builder.Services
            .AddGlobalExceptionHandling()
            .AddCorsServices(builder.Configuration, builder.Environment);

        return builder;
    }

    /// <summary>
    /// Configures the middleware pipeline for a module API host.
    /// </summary>
    public static WebApplication UseModuleHostDefaults(
        this WebApplication app,
        IModuleEndpoints? moduleEndpoints = null)
    {
        // Serilog request logging
        app.UseSerilogRequestLogging();

        // OpenAPI/Swagger (development only)
        if (moduleEndpoints != null)
        {
            app.UseOpenApiForModule(moduleEndpoints);
        }
        else
        {
            app.UseOpenApiSimple();
        }

        // Health checks
        app.MapHealthCheckEndpoint();

        // Exception handling
        app.UseGlobalExceptionHandling();

        // CORS
        app.UseCors();

        // Auth
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }

    /// <summary>
    /// Enforces SSL on database connections in production environments.
    /// Throws if the connection string lacks SSL Mode=Require or VerifyFull.
    /// </summary>
    public static void EnforceSslInProduction(WebApplicationBuilder builder, string databaseConnectionString)
    {
        if (builder.Environment.IsProduction() &&
            !databaseConnectionString.Contains("SSL Mode=Require", StringComparison.OrdinalIgnoreCase) &&
            !databaseConnectionString.Contains("SSL Mode=VerifyFull", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "FATAL: Production database connections MUST use 'SSL Mode=Require' or 'VerifyFull' for FTC compliance.");
        }
    }
}
