using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Respawn.Graph;
using Xunit;

namespace ModularTemplate.IntegrationTests;

// Universal base for integration tests. Boots the real app via Program.cs,
// applies migrations (Development environment), and resets the database
// between tests via Respawn.
//
// Connection strings come from the app's own config pipeline (appsettings.json,
// appsettings.Development.json, environment variables, AWS Secrets Manager).
// The test fixture does NOT hardcode or override connection strings — it reads
// them from the built host's IConfiguration after Program.cs has resolved them.
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
    private string? _resolvedConnectionString;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        // Cache — fallback to in-memory when Redis is unavailable
        builder.UseSetting("ConnectionStrings:Cache", "localhost:6379,abortConnect=false");

        // Encryption — deterministic test key
        const string testKey = "8LQDTJ33CPbaageGk/STuqnge2ZJd/Q+rwvEGbE1X7E=";
        builder.UseSetting("Encryption:Key", testKey);
        Environment.SetEnvironmentVariable("ENCRYPTION_KEY", testKey);

        // Messaging — placeholders so options validation passes
        builder.UseSetting("Messaging:EmbProducer:EventBus", "test-event-bus");
        builder.UseSetting("Messaging:SqsConsumer:SqsQueueUrl",
            "https://sqs.us-east-1.amazonaws.com/000000000000/test-queue");

        // Seeding — disabled, tests seed their own data
        builder.UseSetting("Seeding:Enabled", "false");

        // Disable AWS Secrets Manager in tests — use local appsettings connection strings
        builder.UseSetting("Secrets:UseAws", "false");

        // Enable outbox/inbox so event flow tests can trigger them manually
        builder.UseSetting("Features:Infrastructure:Outbox", "true");
        builder.UseSetting("Features:Infrastructure:Inbox", "true");

        // Module-specific service overrides
        builder.ConfigureTestServices(ConfigureTestServices);
    }

    public async Task InitializeAsync()
    {
        // Build the app — triggers Program.cs (config, DI, migrations)
        _ = Services;

        // Read the connection string from the app's config pipeline.
        // Module fixtures can override ResolveConnectionString to use a module-specific key.
        _resolvedConnectionString = ResolveConnectionString(
            Services.GetRequiredService<IConfiguration>());

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
            await using var connection = new NpgsqlConnection(_resolvedConnectionString);
            await connection.OpenAsync();
            await _respawner.ResetAsync(connection);
        }

        using var scope = Services.CreateScope();
        await SeedReferenceDataAsync(scope);
    }

    // The database connection string resolved by the app's config pipeline.
    // Available after InitializeAsync. Used by module fixtures for Respawn on additional databases.
    protected string ResolvedConnectionString
        => _resolvedConnectionString
           ?? throw new InvalidOperationException("Connection string not yet resolved. Call after InitializeAsync.");

    // Override to register module-specific fakes
    protected virtual void ConfigureTestServices(IServiceCollection services) { }

    // Override to resolve a module-specific connection string from config.
    // Checks TEST_DB_CONNECTION env var first (CI override), then reads from
    // the app's config pipeline (appsettings.json / appsettings.Development.json).
    protected virtual string ResolveConnectionString(IConfiguration configuration)
        => Environment.GetEnvironmentVariable("TEST_DB_CONNECTION")
           ?? configuration.GetConnectionString("Database")
           ?? throw new InvalidOperationException(
               "No database connection string found. Set TEST_DB_CONNECTION env var " +
               "or ensure ConnectionStrings:Database exists in appsettings.");

    // Override to specify which schemas Respawn should clean
    protected virtual string[] GetSchemasToInclude()
        => ["public"];

    // Override to seed reference data after each Respawn reset
    protected virtual Task SeedReferenceDataAsync(IServiceScope scope)
        => Task.CompletedTask;

    private async Task InitializeRespawnerAsync()
    {
        await using var connection = new NpgsqlConnection(_resolvedConnectionString);
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = GetSchemasToInclude(),
            TablesToIgnore = [new Table("migrations", "__EFMigrationsHistory")]
        });
    }
}
