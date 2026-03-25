using Modules.Sales.Integration.Shared;
using Npgsql;
using Respawn;
using Respawn.Graph;

namespace Modules.Sales.EventConsumerTests.Abstractions;

// Test fixture for verifying that integration events from other modules are
// correctly consumed by the Sales module and persisted to its cache tables.
//
// Resets both sales_dev and customer_dev databases between tests so each test
// starts clean. The customer_dev reset covers the customers and messaging schemas
// (outbox messages, consumer tracking) to prevent stale events from prior runs.
public class EventConsumerTestFixture : SalesTestFixtureBase
{
    private const string CustomerDbConnectionString =
        "Host=localhost;Database=customer_dev;Username=postgres;Password=postgres";

    private Respawner? _customerRespawner;

    public override async Task ResetDatabaseAsync()
    {
        await base.ResetDatabaseAsync();
        await ResetCustomerDatabaseAsync();
    }

    // ── Customer database cleanup ──────────────────────────────────

    private async Task ResetCustomerDatabaseAsync()
    {
        if (_customerRespawner is null)
        {
            await using var initConn = new NpgsqlConnection(CustomerDbConnectionString);
            await initConn.OpenAsync();

            _customerRespawner = await Respawner.CreateAsync(initConn, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["customers", "messaging"],
                TablesToIgnore = [new Table("migrations", "__EFMigrationsHistory")]
            });
        }

        await using var conn = new NpgsqlConnection(CustomerDbConnectionString);
        await conn.OpenAsync();
        await _customerRespawner.ResetAsync(conn);
    }
}
