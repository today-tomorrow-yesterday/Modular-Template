using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Rtl.Core.Infrastructure.Persistence;

/// <summary>
/// Base design-time factory for EF Core tooling (dotnet ef migrations add/update).
/// Provides a minimal DbContext configured with Npgsql and snake_case conventions —
/// no DI, no interceptors, no AWS. Just enough for migration scaffolding.
/// </summary>
public abstract class DesignTimeDbContextFactoryBase<TContext> : IDesignTimeDbContextFactory<TContext>
    where TContext : DbContext
{
    private const string DesignTimeConnectionString =
        "Host=localhost;Database=design_time;Username=postgres;Password=postgres";

    public TContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TContext>();

        optionsBuilder
            .UseNpgsql(DesignTimeConnectionString, npgsql =>
                npgsql.MigrationsHistoryTable(
                    HistoryRepository.DefaultTableName, "migrations"))
            .UseSnakeCaseNamingConvention();

        return CreateContext(optionsBuilder.Options);
    }

    protected abstract TContext CreateContext(DbContextOptions<TContext> options);
}
