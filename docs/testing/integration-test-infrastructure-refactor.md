# Integration Test Infrastructure Refactor

## Goal

Refactor the integration test infrastructure so tests only care about business scenarios — not DI registration, database connections, fake adapters, reference data seeding, or cleanup. The plumbing should be invisible.

## Current State

### What exists today

```
Common/test/IntegrationTests/
├── Abstractions/
│   ├── IntegrationTestWebAppFactory.cs      -- boots Program.cs via WebApplicationFactory<Program>
│   ├── BaseIntegrationTest.cs               -- xUnit base with ISender (SampleSales uses this)
│   └── IntegrationTestCollection.cs
├── DatabaseProviders/
│   ├── ITestDatabaseProvider.cs             -- abstraction over InMemory/PostgreSQL
│   ├── TestDatabaseProviderFactory.cs       -- picks provider based on TEST_DB_PROVIDER env var
│   ├── InMemoryTestDatabaseProvider.cs      -- EF InMemory (unused by Sales)
│   └── PostgreSqlTestDatabaseProvider.cs    -- hardcoded connection string, Respawn config

Sales/test/IntegrationTests/
├── Abstractions/
│   ├── SalesTestFactory.cs                  -- extends IntegrationTestWebAppFactory, registers FakeiSeriesAdapter
│   ├── SalesIntegrationTestBase.cs          -- Arrange helpers, reference data seeding, state tracking
│   ├── SalesHttpHelpers.cs                  -- generic HTTP extension methods
│   ├── FakeiSeriesAdapter.cs                -- deterministic fake for external iSeries calls
│   └── IntegrationTestCollection.cs
```

### Problems with current design

1. **Test infrastructure is visible in test code** — `SalesTestFactory` sets env vars, encryption keys, messaging config, environment name. Tests shouldn't know about any of this.

2. **`ITestDatabaseProvider` abstraction is unnecessary** — We always use PostgreSQL. The InMemory provider exists but is never used by Sales. Two code paths to maintain for one outcome.

3. **Reference data seeding uses `ICacheWriteScope` directly in the base class** — Tests shouldn't know about the CacheWriteGuardInterceptor or how to bypass it. That's an infrastructure concern.

4. **`PostgreSqlTestDatabaseProvider` hardcodes connection strings** — Not configurable per module or environment. Separate customer database support was bolted on reactively.

5. **`SalesTestFactory` overrides encryption keys, messaging config, seeding flags** — This is app startup configuration, not test configuration. It belongs in one place.

6. **No auth support** — When endpoints eventually require authorization, there's no `TestAuthHandler` or `GetHttpClient(includeAuth)` pattern.

7. **Respawn schemas are manually maintained** — Schemas were added reactively (`cache`, `packages`) as tests broke. No single source of truth for which schemas to clean.

## Target State

### Design Principles

1. **Tests read like business scenarios** — Only Arrange/Act/Assert with domain language. No DI, no connection strings, no env vars.
2. **One base class per concern level** — Infrastructure base handles all plumbing. Module base adds domain helpers. Test class contains only test logic.
3. **Virtual overrides, not separate provider classes** — `ConfigureServices` and `ResetDatabase` are virtual methods, not strategy pattern abstractions.
4. **Self-contained** — Every test run works on any machine with PostgreSQL running. No dependency on pre-seeded data.
5. **CI-ready** — Works with GitHub Actions PostgreSQL service out of the box.

### Proposed Architecture

```
Common/test/IntegrationTests/
├── IntegrationTestFixture<TEntryPoint>.cs   -- generic base, replaces IntegrationTestWebAppFactory + providers
├── HttpAssert.cs                            -- static helpers: HttpAssert.NotFound(), HttpAssert.BadRequest()
└── TestAuthHandler.cs                       -- fake auth handler (ready for when endpoints require auth)

Sales/test/IntegrationTests/
├── Abstractions/
│   ├── SalesIntegrationTestFixture.cs       -- inherits IntegrationTestFixture<Program>, overrides ConfigureServices/ResetDatabase
│   ├── SalesIntegrationTestBase.cs          -- Arrange helpers, state tracking (no infrastructure concerns)
│   ├── SalesHttpHelpers.cs                  -- stays as-is
│   └── FakeiSeriesAdapter.cs                -- stays as-is
```

### Layer Responsibilities

#### Layer 1: `IntegrationTestFixture<TEntryPoint>` (Common)

The universal base. Any module's integration tests can use this. Handles ALL infrastructure:

```csharp
public abstract class IntegrationTestFixture<TEntryPoint> : WebApplicationFactory<TEntryPoint>, IAsyncLifetime
    where TEntryPoint : class
{
    private Respawner? _respawner;

    // Tests get these — nothing else
    protected HttpClient Client { get; private set; }
    protected IServiceProvider Services => base.Services;

    // ── Lifecycle (invisible to tests) ──────────────────────────

    public async Task InitializeAsync()
    {
        Client = CreateClient();

        // Build app (triggers Program.cs → migrations)
        _ = Services;

        // Initialize Respawn
        await InitializeRespawnerAsync();

        // First reset + seed
        await ResetDatabaseAsync();
    }

    public new async Task DisposeAsync()
    {
        Client.Dispose();
        await base.DisposeAsync();
    }

    // ── Configuration (override in module fixtures) ─────────────

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        // Database — override GetConnectionString() to change
        builder.UseSetting("ConnectionStrings:Database", GetConnectionString());
        builder.UseSetting("ConnectionStrings:Cache", GetCacheConnectionString());

        // Encryption — deterministic test key
        var testKey = "8LQDTJ33CPbaageGk/STuqnge2ZJd/Q+rwvEGbE1X7E=";
        builder.UseSetting("Encryption:Key", testKey);
        Environment.SetEnvironmentVariable("ENCRYPTION_KEY", testKey);

        // Messaging — placeholders so validation passes
        builder.UseSetting("Messaging:EmbProducer:EventBus", "test-event-bus");
        builder.UseSetting("Messaging:SqsConsumer:SqsQueueUrl",
            "https://sqs.us-east-1.amazonaws.com/000000000000/test-queue");

        // Seeding — disabled by default (tests seed their own data)
        builder.UseSetting("Seeding:Enabled", "false");

        // Auth — register TestAuthHandler for future use
        builder.ConfigureTestServices(services =>
        {
            ConfigureTestServices(services);
        });
    }

    // ── Virtual extension points ────────────────────────────────

    // Override to register module-specific fakes (e.g., FakeiSeriesAdapter)
    protected virtual void ConfigureTestServices(IServiceCollection services) { }

    // Override to change the database connection
    protected virtual string GetConnectionString()
        => "Host=localhost;Database=sales_dev;Username=postgres;Password=postgres";

    protected virtual string GetCacheConnectionString()
        => "localhost:6379,abortConnect=false";

    // Override to specify which schemas Respawn should clean
    protected virtual string[] GetSchemasToInclude()
        => ["public"];

    // Override to specify tables Respawn should preserve
    protected virtual string[] GetTablesToIgnore()
        => [];

    // Override to seed reference data after each Respawn reset
    protected virtual Task SeedReferenceDataAsync(IServiceScope scope)
        => Task.CompletedTask;

    // ── Reset (called before each test) ─────────────────────────

    public async Task ResetDatabaseAsync()
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

    private async Task InitializeRespawnerAsync()
    {
        await using var connection = new NpgsqlConnection(GetConnectionString());
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = GetSchemasToInclude(),
            TablesToIgnore = GetTablesToIgnore()
                .Select(t => new Table(t))
                .Append(new Table("migrations", "__EFMigrationsHistory"))
                .ToArray()
        });
    }

    // ── Auth helper (for future use) ────────────────────────────

    protected HttpClient GetHttpClient(bool includeAuthorizationHeader = true)
    {
        if (includeAuthorizationHeader)
        {
            Client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "test-token");
        }
        return Client;
    }
}
```

#### Layer 2: `SalesIntegrationTestFixture` (Module)

Module-specific fixture. Overrides only what's different for the Sales module:

```csharp
public sealed class SalesIntegrationTestFixture : IntegrationTestFixture<Program>
{
    protected override void ConfigureTestServices(IServiceCollection services)
    {
        // Replace iSeries adapter with deterministic fake
        services.AddSingleton<IiSeriesAdapter, FakeiSeriesAdapter>();
    }

    protected override string[] GetSchemasToInclude()
        => ["sales", "packages", "cache"];

    protected override async Task SeedReferenceDataAsync(IServiceScope scope)
    {
        var db = scope.ServiceProvider.GetRequiredService<SalesDbContext>();
        var cacheScope = scope.ServiceProvider.GetRequiredService<ICacheWriteScope>();

        using (cacheScope.AllowWrites())
        {
            var retailLocation = RetailLocationCache.CreateHomeCenter(
                100, "Test HC", "TN", "37801", isActive: true);
            db.Set<RetailLocationCache>().Add(retailLocation);

            db.Set<CustomerCache>().Add(new CustomerCache
            {
                RefPublicId = TestData.CustomerId,
                HomeCenterNumber = 100,
                LifecycleStage = LifecycleStage.Customer,
                DisplayName = "Test Customer",
                FirstName = "Test",
                LastName = "Customer",
                LastSyncedAtUtc = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
        }
    }
}

// Known test data — single source of truth
public static class TestData
{
    public static readonly Guid CustomerId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
    public const int HomeCenterNumber = 100;
    public const int AuthorizedUserId1 = 1;
    public const int AuthorizedUserId2 = 2;
}
```

#### Layer 3: `SalesIntegrationTestBase` (Test base)

Pure domain helpers. No infrastructure knowledge:

```csharp
[Collection("SalesIntegration")]
public abstract class SalesIntegrationTestBase(SalesIntegrationTestFixture fixture) : IAsyncLifetime
{
    protected readonly HttpClient Client = fixture.CreateClient();

    // State set by Arrange helpers
    protected Guid SaleId { get; private set; }
    protected int SaleNumber { get; private set; }
    protected Guid PackageId { get; private set; }
    protected Guid DeliveryAddressId { get; private set; }

    public async Task InitializeAsync() => await fixture.ResetDatabaseAsync();
    public Task DisposeAsync() { Client.Dispose(); return Task.CompletedTask; }

    // ── Arrange helpers (pure HTTP, no infrastructure) ───────────

    protected async Task ArrangeSaleAsync() { /* POST /api/v1/sales */ }
    protected async Task ArrangeSaleWithDeliveryAsync() { /* + POST delivery-address */ }
    protected async Task ArrangeSaleWithPackageAsync() { /* + POST packages */ }
    protected async Task ArrangeSaleWithHomeAsync() { /* + PUT home */ }

    // ── Package shortcut ────────────────────────────────────────

    protected async Task<PackageDetailResponse> GetPackageAsync()
        => await Client.GetPackageAsync(PackageId);
}
```

#### Layer 4: Test class (pure business logic)

```csharp
public class CreateSaleTests(SalesIntegrationTestFixture fixture) : SalesIntegrationTestBase(fixture)
{
    private const string Endpoint = "/api/v1/sales";

    [Fact]
    public async Task Should_ReturnCreated_WhenRequestIsValid()
    {
        // Arrange
        var request = new CreateSaleRequest(TestData.CustomerId, TestData.HomeCenterNumber);

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
```

### What this eliminates

| Current | After refactor |
|---------|---------------|
| `ITestDatabaseProvider` interface | Deleted — virtual methods instead |
| `TestDatabaseProviderFactory` | Deleted |
| `InMemoryTestDatabaseProvider` | Deleted |
| `PostgreSqlTestDatabaseProvider` | Deleted — logic moves into `IntegrationTestFixture` |
| `IntegrationTestWebAppFactory` | Replaced by `IntegrationTestFixture<TEntryPoint>` |
| `TEST_DB_PROVIDER` env var | Eliminated |
| `SalesTestFactory` setting env vars | Moves to virtual overrides in `SalesIntegrationTestFixture` |
| `SalesIntegrationTestBase` doing `ICacheWriteScope` | Moves to `SalesIntegrationTestFixture.SeedReferenceDataAsync` |

### `HttpAssert` helper (new)

```csharp
public static class HttpAssert
{
    public static void IsOk(HttpResponseMessage response)
        => Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    public static void IsCreated(HttpResponseMessage response)
        => Assert.Equal(HttpStatusCode.Created, response.StatusCode);

    public static void IsNotFound(HttpResponseMessage response)
        => Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

    public static void IsBadRequest(HttpResponseMessage response)
        => Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

    public static void IsConflict(HttpResponseMessage response)
        => Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

    public static void IsUnauthorized(HttpResponseMessage response)
        => Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
}
```

### `TestData` vs scattered constants

Currently `SalesTestFactory` has `TestCustomerId`, `TestHomeCenterNumber`, etc. These move to a `TestData` static class — one place to find all known test values.

## Files to delete

- `Common/test/IntegrationTests/DatabaseProviders/ITestDatabaseProvider.cs`
- `Common/test/IntegrationTests/DatabaseProviders/TestDatabaseProviderFactory.cs`
- `Common/test/IntegrationTests/DatabaseProviders/InMemoryTestDatabaseProvider.cs`
- `Common/test/IntegrationTests/DatabaseProviders/PostgreSqlTestDatabaseProvider.cs`

## Files to create

- `Common/test/IntegrationTests/IntegrationTestFixture.cs`
- `Common/test/IntegrationTests/HttpAssert.cs`
- `Common/test/IntegrationTests/TestAuthHandler.cs` (ready for future auth testing)

## Files to modify

- `Sales/test/IntegrationTests/Abstractions/SalesIntegrationTestBase.cs` — remove infrastructure, keep Arrange helpers
- `Sales/test/IntegrationTests/Abstractions/SalesTestFactory.cs` → rename to `SalesIntegrationTestFixture.cs`
- All test files — update constructor to use `SalesIntegrationTestFixture`

## Migration Strategy

1. Create `IntegrationTestFixture<TEntryPoint>` alongside existing infrastructure (no breaking changes)
2. Create `SalesIntegrationTestFixture` extending it
3. Update `SalesIntegrationTestBase` to use the new fixture
4. Update all test files (mechanical — just constructor parameter type change)
5. Delete old provider files
6. Run all 40 tests — verify green
7. Add `HttpAssert` helper
8. Add `TestAuthHandler` (dormant until needed)

## Open Questions

1. **Should `SeedReferenceDataAsync` use a fixed `TestData.CustomerId` GUID or generate a new one each test?** Fixed is more predictable and debuggable. Generated avoids any possibility of stale data conflicts.

2. **Should `IntegrationTestFixture` support multiple databases?** The current `PostgreSqlTestDatabaseProvider` was extended with a separate customer DB connection. Should the fixture support `GetAdditionalConnectionStrings()` for multi-database modules?

3. **Should `HttpAssert` return the response for chaining?** e.g., `var body = HttpAssert.IsCreated(response).ReadAsJsonAsync<T>()`

4. **Should we add a `TestDataSeeder` service pattern?** Instead of overriding `SeedReferenceDataAsync`, register an `ITestDataSeeder` in DI that runs automatically. More extensible but adds complexity.

## Reference Implementation

The patterns above are inspired by a production integration test suite (CMH.CES.Infrastructure.Testing) that uses:
- `IntegrationTestFixture<TEntryPoint>` as a generic base
- Virtual `ConfigureServices` and `ResetDatabase`
- `GetHttpClient(bool includeAuth)` for auth toggling
- `HttpAssert` static helpers for clean status code assertions
- Typed API client wrappers for readable test code
- `DataStore` static class for known test constants
