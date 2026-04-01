using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using ModularTemplate.Infrastructure.Secrets;

namespace ModularTemplate.Infrastructure.Persistence;

/// <summary>
/// Base design-time factory for EF Core tooling (dotnet ef migrations add/update).
/// Builds configuration from appsettings and resolves the connection string
/// through DatabaseConnectionResolver — same path as runtime.
/// </summary>
public abstract class DesignTimeDbContextFactoryBase<TContext> : IDesignTimeDbContextFactory<TContext>
    where TContext : DbContext
{
    public TContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = DatabaseConnectionResolver.Resolve(configuration);

        var optionsBuilder = new DbContextOptionsBuilder<TContext>();

        optionsBuilder
            .UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable(
                    HistoryRepository.DefaultTableName, "migrations"))
            .UseSnakeCaseNamingConvention();

        return CreateContext(optionsBuilder.Options);
    }

    protected abstract TContext CreateContext(DbContextOptions<TContext> options);
}
