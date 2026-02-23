using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace Rtl.Core.Api.Shared;

/// <summary>
/// Extension methods for database migration management.
/// </summary>
public static class DatabaseMigrationExtensions
{
    /// <summary>
    /// Applies pending migrations for a single DbContext when running in Development environment.
    /// Use this overload for per-module API hosts that have only one DbContext.
    /// </summary>
    /// <remarks>
    /// In non-development environments (Staging, Production), migrations should be applied
    /// through CI/CD pipelines for controlled deployment.
    /// </remarks>
    public static IApplicationBuilder ApplyMigrations<TDbContext>(
        this IApplicationBuilder app,
        IHostEnvironment environment,
        string connectionString)
        where TDbContext : DbContext
    {
        if (!environment.IsDevelopment())
        {
            return app;
        }

        EnsureDatabaseExists(connectionString);

        using var scope = app.ApplicationServices.CreateScope();
        ApplyMigration<TDbContext>(scope);

        return app;
    }

    /// <summary>
    /// Applies pending migrations for multiple DbContexts with optional module-specific database support.
    /// Use this overload for the main API host that manages all modules.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method supports multi-database deployments where each module can have its own database.
    /// It reads module-specific connection strings from configuration using the pattern
    /// <c>Modules:{ModuleName}:ConnectionStrings:Database</c> and falls back to the default
    /// connection string if not specified.
    /// </para>
    /// <para>
    /// In non-development environments (Staging, Production), migrations should be applied
    /// through CI/CD pipelines for controlled deployment.
    /// </para>
    /// </remarks>
    public static IApplicationBuilder ApplyMigrations(
        this IApplicationBuilder app,
        IHostEnvironment environment,
        IConfiguration configuration,
        string defaultConnectionString,
        params (string ModuleName, Type DbContextType)[] dbContexts)
    {
        if (!environment.IsDevelopment())
        {
            return app;
        }

        // Collect all unique connection strings
        HashSet<string> connectionStrings = [];
        foreach (var (moduleName, _) in dbContexts)
        {
            var connStr = GetModuleConnectionString(configuration, moduleName, defaultConnectionString);
            connectionStrings.Add(connStr);
        }

        // Ensure each unique database exists
        foreach (var connectionString in connectionStrings)
        {
            EnsureDatabaseExists(connectionString);
        }

        // Apply migrations for each DbContext
        using var scope = app.ApplicationServices.CreateScope();
        foreach (var (_, dbContextType) in dbContexts)
        {
            ApplyMigration(scope, dbContextType);
        }

        return app;
    }

    /// <summary>
    /// Gets a module-specific connection string from configuration with fallback to default.
    /// </summary>
    public static string GetModuleConnectionString(
        IConfiguration configuration,
        string moduleName,
        string defaultConnectionString)
    {
        return configuration[$"Modules:{moduleName}:ConnectionStrings:Database"]
               ?? defaultConnectionString;
    }

    /// <summary>
    /// Ensures the database specified in the connection string exists, creating it if necessary.
    /// </summary>
    public static void EnsureDatabaseExists(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var databaseName = builder.Database;

        // Connect to the default 'postgres' database to check/create our target database
        builder.Database = "postgres";

        using var connection = new NpgsqlConnection(builder.ConnectionString);
        connection.Open();

        // Check if database exists
        using var checkCmd = connection.CreateCommand();
        checkCmd.CommandText = "SELECT 1 FROM pg_database WHERE datname = @dbname";
        checkCmd.Parameters.AddWithValue("dbname", databaseName!);

        var exists = checkCmd.ExecuteScalar() != null;

        if (!exists)
        {
            // Create the database
            using var createCmd = connection.CreateCommand();
            createCmd.CommandText = $"CREATE DATABASE \"{databaseName}\"";
            createCmd.ExecuteNonQuery();

            Console.WriteLine($"Created database: {databaseName}");
        }
    }

    private static void ApplyMigration<TDbContext>(IServiceScope scope)
        where TDbContext : DbContext
    {
        using var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        context.Database.Migrate();
    }

    private static void ApplyMigration(IServiceScope scope, Type dbContextType)
    {
        using var context = (DbContext)scope.ServiceProvider.GetRequiredService(dbContextType);
        context.Database.Migrate();
    }
}
