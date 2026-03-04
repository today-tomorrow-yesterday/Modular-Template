using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Infrastructure.Application;
using Rtl.Core.Infrastructure.Auditing;
using Rtl.Core.Infrastructure.Authentication;
using Rtl.Core.Infrastructure.Authorization;
using Rtl.Core.Infrastructure.Caching;
using Rtl.Core.Infrastructure.Clock;
using Rtl.Core.Infrastructure.EventBus;
using Rtl.Core.Infrastructure.FeatureManagement;
using Rtl.Core.Infrastructure.ISeries;
using Rtl.Core.Infrastructure.Persistence;
using Rtl.Core.Infrastructure.Quartz;
using Rtl.Core.Infrastructure.Resilience;
using Rtl.Core.Infrastructure.Secrets;

namespace Rtl.Core.Infrastructure;

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
        services.AddOptions<Rtl.Core.Infrastructure.Security.EncryptionOptions>()
            .Bind(configuration.GetSection(Rtl.Core.Infrastructure.Security.EncryptionOptions.SectionName))
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
            .AddSecretProvider(configuration, environment)
            .AddISeriesAdapter(configuration);

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
