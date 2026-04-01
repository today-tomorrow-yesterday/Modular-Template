using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using ModularTemplate.Application.Caching;
using StackExchange.Redis;

namespace ModularTemplate.Infrastructure.Caching;

/// <summary>
/// Extension methods for configuring caching services.
/// </summary>
internal static class Startup
{
    /// <summary>
    /// Adds caching services to the service collection.
    /// </summary>
    internal static IServiceCollection AddCachingServices(
        this IServiceCollection services,
        IConfiguration configuration,
        string redisConnectionString)
    {
        services.AddOptions<CachingOptions>()
            .Bind(configuration.GetSection(CachingOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        try
        {
            IConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
            services.AddSingleton(connectionMultiplexer);
            services.AddStackExchangeRedisCache(options =>
                options.ConnectionMultiplexerFactory = () => Task.FromResult(connectionMultiplexer));
        }
        catch (Exception ex)
        {
            LogRedisFallback(ex, redisConnectionString);
            services.AddDistributedMemoryCache();
        }

        services.TryAddScoped<ICacheService, CacheService>();
        return services;
    }

    private static void LogRedisFallback(Exception ex, string connectionString)
    {
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger("CachingStartup");
        logger.LogWarning(
            ex,
            "Failed to connect to Redis at '{ConnectionString}'. Falling back to in-memory distributed cache. " +
            "This is not suitable for production multi-instance deployments",
            connectionString);
    }
}
