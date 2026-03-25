using Microsoft.AspNetCore.Hosting;
using Npgsql;
using Respawn;
using Respawn.Graph;

namespace Rtl.Core.IntegrationTests.DatabaseProviders;

public sealed class PostgreSqlTestDatabaseProvider : ITestDatabaseProvider
{
    private const string ConnectionString = "Host=localhost;Database=sales_dev;Username=postgres;Password=postgres";
    private const string CacheConnectionString = "localhost:6379";

    private Respawner? _respawner;
    private NpgsqlConnection? _dbConnection;

    public string ProviderName => "PostgreSQL (Local)";

    public void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Database", ConnectionString);
        builder.UseSetting("ConnectionStrings:Cache", CacheConnectionString);
    }

    public void SetServiceProvider(IServiceProvider serviceProvider)
    {
        // Not needed for PostgreSQL - Respawn handles reset
    }

    public Task InitializeAsync()
    {
        // No container startup needed for local PostgreSQL
        return Task.CompletedTask;
    }

    private async Task InitializeRespawnerAsync()
    {
        _dbConnection = new NpgsqlConnection(ConnectionString);
        await _dbConnection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["sample", "orders", "sales", "packages", "cache"],
            TablesToIgnore = [new Table("migrations", "__EFMigrationsHistory")]
        });
    }

    public async Task ResetAsync()
    {
        if (_respawner is null || _dbConnection is null)
        {
            await InitializeRespawnerAsync();
        }

        await _respawner!.ResetAsync(_dbConnection!);
    }

    public async Task DisposeAsync()
    {
        if (_dbConnection is not null)
        {
            await _dbConnection.DisposeAsync();
        }
    }
}
