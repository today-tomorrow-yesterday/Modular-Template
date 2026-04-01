using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using ModularTemplate.Infrastructure.Auditing.Interceptors;
using ModularTemplate.Infrastructure.Caching;
using ModularTemplate.Infrastructure.Outbox.Persistence;

namespace ModularTemplate.Infrastructure.Persistence;

/// <summary>
/// Extension methods for configuring persistence services.
/// </summary>
public static class Startup
{
    /// <summary>
    /// Adds PostgreSQL data source services to the service collection.
    /// </summary>
    internal static IServiceCollection AddPersistenceServices(
        this IServiceCollection services,
        string connectionString)
    {
        var npgsqlDataSource = new NpgsqlDataSourceBuilder(connectionString).Build();
        services.TryAddSingleton(npgsqlDataSource);
        return services;
    }

    /// <summary>
    /// Adds a module DbContext with standard conventions.
    /// </summary>
    public static IServiceCollection AddModuleDbContext<TDbContext>(
        this IServiceCollection services,
        string connectionString,
        string schema,
        Action<DbContextOptionsBuilder>? configure = null)
        where TDbContext : DbContext
    {
        services.AddDbContext<TDbContext>((sp, options) =>
        {
            options.UseNpgsql(
                connectionString,
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(
                    HistoryRepository.DefaultTableName,
                    "migrations"))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(
                    sp.GetRequiredService<CacheWriteGuardInterceptor>(),
                    sp.GetRequiredService<SoftDeleteInterceptor>(),
                    sp.GetRequiredService<AuditableEntitiesInterceptor>(),
                    sp.GetRequiredService<AuditTrailInterceptor>(),
                    sp.GetRequiredService<InsertOutboxMessagesInterceptor>());

            // Apply any module-specific configuration
            configure?.Invoke(options);
        });

        return services;
    }
}
