using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using ModularTemplate.Application.Persistence;
using ModularTemplate.Infrastructure.Application;
using ModularTemplate.Infrastructure.Auditing;
using ModularTemplate.Infrastructure.Authentication;
using ModularTemplate.Infrastructure.Authorization;
using ModularTemplate.Infrastructure.Caching;
using ModularTemplate.Infrastructure.Clock;
using ModularTemplate.Infrastructure.EventBus;
using ModularTemplate.Infrastructure.FeatureManagement;
using ModularTemplate.Infrastructure.Persistence;
using ModularTemplate.Infrastructure.Quartz;
using ModularTemplate.Infrastructure.Resilience;
using ModularTemplate.Infrastructure.Secrets;

namespace ModularTemplate.Infrastructure;

/// <summary>
/// Extension methods for configuring the infrastructure layer.
/// </summary>
public static class InfrastructureConfiguration
{
    /// <summary>
    /// Adds infrastructure layer services.
    /// </summary>
    public static IServiceCollection AddCommonInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        string databaseConnectionString,
        string redisConnectionString)
    {
        services.AddOptions<ModularTemplate.Infrastructure.Security.EncryptionOptions>()
            .Bind(configuration.GetSection(ModularTemplate.Infrastructure.Security.EncryptionOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Each feature registers itself via its Startup class
        services
            .AddClockServices()
            .AddResilienceServices(configuration)
            .AddFeatureManagementServices(configuration)
            .AddAuditingServices()
            .AddPersistenceServices(databaseConnectionString)
            .AddQuartzServices()
            .AddCachingServices(configuration, redisConnectionString)
            .AddMessagingServices(configuration, environment)
            .AddAuthenticationServices(configuration)
            .AddAuthorizationServices()
            .AddSecretProvider(configuration, environment);

        return services;
    }

    /// <summary>
    /// Registers a module-specific database data source and connection factory.
    /// </summary>
    public static IServiceCollection AddModuleDataSource<TModule>(
        this IServiceCollection services,
        string connectionString)
        where TModule : class
    {
        var dataSource = new NpgsqlDataSourceBuilder(connectionString).Build();
        services.AddKeyedSingleton<NpgsqlDataSource>(typeof(TModule), dataSource);

        // Use factory delegate to resolve the keyed NpgsqlDataSource
        services.AddScoped<IDbConnectionFactory<TModule>>(sp =>
        {
            var keyedDataSource = sp.GetRequiredKeyedService<NpgsqlDataSource>(typeof(TModule));
            return new DbConnectionFactory<TModule>(keyedDataSource);
        });

        return services;
    }
}
