using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;
using ModularTemplate.Api.Shared.HealthChecks;
using ModularTemplate.Infrastructure.Application;

namespace ModularTemplate.Api.Shared;

/// <summary>
/// Extension methods for health check configuration.
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Adds health check services for database and cache connectivity.
    /// </summary>
    public static IServiceCollection AddHealthChecks(
        this IServiceCollection services,
        string databaseConnectionString,
        string? cacheConnectionString = null)
    {
        var healthChecks = services.AddHealthChecks()
            .AddNpgSql(databaseConnectionString, name: "database")
            .AddCheck("database-ssl", new SslConnectionHealthCheck(databaseConnectionString), tags: ["database", "ssl"]);

        if (!string.IsNullOrEmpty(cacheConnectionString))
        {
            healthChecks.AddRedis(cacheConnectionString, name: "cache");
        }

        return services;
    }

    /// <summary>
    /// Adds granular health checks for outbox lag, inbox lag, queue depth, and per-module health.
    /// </summary>
    public static IServiceCollection AddGranularHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Get module schemas from ApplicationOptions (single source of truth)
        var moduleSchemas = GetModuleSchemas(configuration);

        // Configure options from configuration with validation
        // Use PostConfigure to inject schemas from ApplicationOptions
        services.AddOptions<OutboxLagHealthCheckOptions>()
            .Bind(configuration.GetSection(OutboxLagHealthCheckOptions.SectionName))
            .PostConfigure(options => options.Schemas = moduleSchemas)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<InboxLagHealthCheckOptions>()
            .Bind(configuration.GetSection(InboxLagHealthCheckOptions.SectionName))
            .PostConfigure(options => options.Schemas = moduleSchemas)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<SqsQueueDepthHealthCheckOptions>()
            .Bind(configuration.GetSection(SqsQueueDepthHealthCheckOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Register health checks
        services.AddHealthChecks()
            .AddCheck<OutboxLagHealthCheck>(
                "outbox-lag",
                failureStatus: HealthStatus.Degraded,
                tags: ["messaging", "outbox"])
            .AddCheck<InboxLagHealthCheck>(
                "inbox-lag",
                failureStatus: HealthStatus.Degraded,
                tags: ["messaging", "inbox"])
            .AddCheck<SqsQueueDepthHealthCheck>(
                "sqs-queue-depth",
                failureStatus: HealthStatus.Degraded,
                tags: ["messaging", "sqs"]);

        // Add per-module health checks
        services.AddModuleHealthChecks(configuration);

        return services;
    }

    /// <summary>
    /// Adds per-module health checks for each configured module.
    /// </summary>
    private static IServiceCollection AddModuleHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Get module schemas from ApplicationOptions (single source of truth)
        var moduleSchemas = GetModuleSchemas(configuration);

        var healthChecksBuilder = services.AddHealthChecks();

        foreach (var schema in moduleSchemas)
        {
            var moduleName = char.ToUpperInvariant(schema[0]) + schema[1..];
            var checkName = $"module-{schema}";

            // Register a factory for each module health check
            healthChecksBuilder.Add(new HealthCheckRegistration(
                checkName,
                sp =>
                {
                    var dataSource = sp.GetRequiredService<NpgsqlDataSource>();
                    var options = new ModuleHealthCheckOptions
                    {
                        ModuleName = moduleName,
                        Schema = schema
                    };

                    // Allow per-module configuration overrides
                    var moduleSection = configuration.GetSection($"HealthChecks:Module:{moduleName}");
                    if (moduleSection.Exists())
                    {
                        options.OutboxDegradedThresholdSeconds =
                            moduleSection.GetValue("OutboxDegradedThresholdSeconds", options.OutboxDegradedThresholdSeconds);
                        options.OutboxUnhealthyThresholdSeconds =
                            moduleSection.GetValue("OutboxUnhealthyThresholdSeconds", options.OutboxUnhealthyThresholdSeconds);
                        options.OutboxDegradedCountThreshold =
                            moduleSection.GetValue("OutboxDegradedCountThreshold", options.OutboxDegradedCountThreshold);
                        options.OutboxUnhealthyCountThreshold =
                            moduleSection.GetValue("OutboxUnhealthyCountThreshold", options.OutboxUnhealthyCountThreshold);
                    }

                    return new ModuleHealthCheck(dataSource, options);
                },
                failureStatus: HealthStatus.Degraded,
                tags: ["module", schema]));
        }

        return services;
    }

    /// <summary>
    /// Maps the health check endpoint with UI response writer.
    /// Explicitly allows anonymous access for health probes.
    /// </summary>
    public static IEndpointRouteBuilder MapHealthCheckEndpoint(
        this IEndpointRouteBuilder app,
        string pattern = "/health")
    {
        app.MapHealthChecks(pattern, new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        }).AllowAnonymous();

        return app;
    }

    /// <summary>
    /// Maps the liveness probe endpoint at /health/live.
    /// This is a minimal check that just verifies the application responds.
    /// Use this for Kubernetes liveness probes to determine if the container should be restarted.
    /// </summary>
    public static IEndpointRouteBuilder MapLivenessProbeEndpoint(
        this IEndpointRouteBuilder app,
        string pattern = "/health/live")
    {
        app.MapHealthChecks(pattern, new HealthCheckOptions
        {
            // No health checks - just returns 200 if the app is running
            Predicate = _ => false,
            ResponseWriter = async (context, _) =>
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"status\":\"Healthy\"}");
            }
        }).AllowAnonymous();

        return app;
    }

    /// <summary>
    /// Maps the readiness probe endpoint at /health/ready.
    /// This checks database and cache connectivity to determine if the app can handle requests.
    /// Use this for Kubernetes readiness probes to determine if traffic should be routed to the pod.
    /// </summary>
    public static IEndpointRouteBuilder MapReadinessProbeEndpoint(
        this IEndpointRouteBuilder app,
        string pattern = "/health/ready")
    {
        app.MapHealthChecks(pattern, new HealthCheckOptions
        {
            // Only run database and cache checks
            Predicate = check => check.Name == "database" || check.Name == "cache",
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        }).AllowAnonymous();

        return app;
    }

    /// <summary>
    /// Maps the startup probe endpoint at /health/startup.
    /// This checks only database connectivity to determine if the app has completed startup.
    /// Use this for Kubernetes startup probes to give slow-starting apps time to initialize.
    /// </summary>
    public static IEndpointRouteBuilder MapStartupProbeEndpoint(
        this IEndpointRouteBuilder app,
        string pattern = "/health/startup")
    {
        app.MapHealthChecks(pattern, new HealthCheckOptions
        {
            // Only run database check for startup
            Predicate = check => check.Name == "database",
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        }).AllowAnonymous();

        return app;
    }

    /// <summary>
    /// Maps a filtered health check endpoint that only runs health checks with specific tags.
    /// </summary>
    public static IEndpointRouteBuilder MapTaggedHealthCheckEndpoint(
        this IEndpointRouteBuilder app,
        string pattern,
        params string[] tags)
    {
        app.MapHealthChecks(pattern, new HealthCheckOptions
        {
            Predicate = check => tags.Any(tag => check.Tags.Contains(tag)),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        }).AllowAnonymous();

        return app;
    }

    /// <summary>
    /// Gets module schemas from ApplicationOptions configuration.
    /// </summary>
    private static string[] GetModuleSchemas(IConfiguration configuration)
    {
        var applicationOptions = configuration
            .GetSection(ApplicationOptions.SectionName)
            .Get<ApplicationOptions>();

        // GetModules() returns Modules array if set, otherwise derives from DatabaseName/Name
        // If config is invalid, ValidateOnStart() will fail at app.Build()
        return applicationOptions?.GetModules() ?? [];
    }
}
