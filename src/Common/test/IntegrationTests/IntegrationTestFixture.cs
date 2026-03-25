using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Respawn.Graph;
using Xunit;

namespace Rtl.Core.IntegrationTests;

// Universal base for integration tests. Boots the real app via Program.cs,
// applies migrations (Development environment), and resets the database
// between tests via Respawn.
//
// Module fixtures override virtual methods to:
// - Register fake services (ConfigureTestServices)
// - Specify which schemas to clean (GetSchemasToInclude)
// - Seed reference data after each reset (SeedReferenceDataAsync)
//
// Tests never see connection strings, env vars, or DI configuration.
public class IntegrationTestFixture<TEntryPoint> : WebApplicationFactory<TEntryPoint>, IAsyncLifetime
    where TEntryPoint : class
{
    private Respawner? _respawner;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.UseSetting("ConnectionStrings:Database", GetConnectionString());
        builder.UseSetting("ConnectionStrings:Cache", "localhost:6379,abortConnect=false");

        const string testKey = "8LQDTJ33CPbaageGk/STuqnge2ZJd/Q+rwvEGbE1X7E=";
        builder.UseSetting("Encryption:Key", testKey);
        Environment.SetEnvironmentVariable("ENCRYPTION_KEY", testKey);

        builder.UseSetting("Messaging:EmbProducer:EventBus", "test-event-bus");
        builder.UseSetting("Messaging:SqsConsumer:SqsQueueUrl",
            "https://sqs.us-east-1.amazonaws.com/000000000000/test-queue");

        builder.UseSetting("Seeding:Enabled", "false");

        // Enable outbox/inbox so event flow tests can trigger them manually
        builder.UseSetting("Features:Infrastructure:Outbox", "true");
        builder.UseSetting("Features:Infrastructure:Inbox", "true");

        builder.ConfigureTestServices(ConfigureTestServices);
    }

    public async Task InitializeAsync()
    {
        _ = Services;
        await InitializeRespawnerAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }

    public virtual async Task ResetDatabaseAsync()
    {
        if (_respawner is not null)
        {
            await using var connection = new NpgsqlConnection(GetConnectionString());
            await connection.OpenAsync();
            await _respawner.ResetAsync(connection);
        }

        using var scope = Services.CreateScope();
        await SeedReferenceDataAsync(scope);
    }

    // Override to register module-specific fakes
    protected virtual void ConfigureTestServices(IServiceCollection services) { }

    // Override to change the database connection
    protected virtual string GetConnectionString()
        => "Host=localhost;Database=sales_dev;Username=postgres;Password=postgres";

    // Override to specify which schemas Respawn should clean
    protected virtual string[] GetSchemasToInclude()
        => ["public"];

    // Override to seed reference data after each Respawn reset
    protected virtual Task SeedReferenceDataAsync(IServiceScope scope)
        => Task.CompletedTask;

    private async Task InitializeRespawnerAsync()
    {
        await using var connection = new NpgsqlConnection(GetConnectionString());
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = GetSchemasToInclude(),
            TablesToIgnore = [new Table("migrations", "__EFMigrationsHistory")]
        });
    }
}
