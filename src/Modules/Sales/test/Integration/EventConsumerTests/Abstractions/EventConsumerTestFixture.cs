using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Sales.Integration.Shared;
using Npgsql;
using Respawn;
using Respawn.Graph;

namespace Modules.Sales.EventConsumerTests.Abstractions;

// Test fixture for verifying that integration events from other modules are
// correctly consumed by the Sales module and persisted to its cache tables.
//
// Resets both the Sales and Customer databases between tests so each test
// starts clean. The Customer database reset covers the customers and messaging
// schemas (outbox messages, consumer tracking) to prevent stale events.
//
// Connection strings come from the app's config pipeline — no hardcoded values.
public class EventConsumerTestFixture : SalesTestFixtureBase
{
    private Respawner? _customerRespawner;
    private string? _customerConnectionString;

    public override async Task ResetDatabaseAsync()
    {
        await base.ResetDatabaseAsync();
        await ResetCustomerDatabaseAsync();
    }

    // Reads the Customer module's connection string from the app's config.
    // Checks Modules:Customer:ConnectionStrings:Database first, falls back to ConnectionStrings:Database.
    private string GetCustomerConnectionString()
    {
        if (_customerConnectionString is not null)
            return _customerConnectionString;

        var config = Services.GetRequiredService<IConfiguration>();
        _customerConnectionString =
            config["Modules:Customer:ConnectionStrings:Database"]
            ?? config.GetConnectionString("Database")
            ?? throw new InvalidOperationException(
                "No Customer database connection string found in configuration.");

        return _customerConnectionString;
    }

    private async Task ResetCustomerDatabaseAsync()
    {
        var connectionString = GetCustomerConnectionString();

        if (_customerRespawner is null)
        {
            await using var initConn = new NpgsqlConnection(connectionString);
            await initConn.OpenAsync();

            _customerRespawner = await Respawner.CreateAsync(initConn, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["customers", "messaging"],
                TablesToIgnore = [new Table("migrations", "__EFMigrationsHistory")]
            });
        }

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();
        await _customerRespawner.ResetAsync(conn);
    }
}
