# Integration Test Infrastructure Refactor

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Flatten the integration test infrastructure so tests never see DI, connection strings, env vars, or cache scopes — only business scenarios with Arrange/Act/Assert.

**Architecture:** Replace the `ITestDatabaseProvider` strategy pattern and `IntegrationTestWebAppFactory` with a single generic `IntegrationTestFixture<TEntryPoint>` that uses virtual methods for extension. Module fixtures override `ConfigureTestServices`, `GetSchemasToInclude`, and `SeedReferenceDataAsync`. Test base classes contain only Arrange helpers and state tracking. Add `HttpAssert` static helper for clean status code assertions.

**Tech Stack:** .NET 10, xUnit 2.9, WebApplicationFactory, Respawn 7, Npgsql, PostgreSQL

**Spec:** `docs/testing/integration-test-infrastructure-refactor.md`

---

## File Structure

### Files to Delete
- `src/Common/test/IntegrationTests/DatabaseProviders/ITestDatabaseProvider.cs`
- `src/Common/test/IntegrationTests/DatabaseProviders/TestDatabaseProviderFactory.cs`
- `src/Common/test/IntegrationTests/DatabaseProviders/InMemoryTestDatabaseProvider.cs`
- `src/Common/test/IntegrationTests/DatabaseProviders/PostgreSqlTestDatabaseProvider.cs`
- `src/Common/test/IntegrationTests/Abstractions/IntegrationTestWebAppFactory.cs`

### Files to Create
- `src/Common/test/IntegrationTests/IntegrationTestFixture.cs` — generic base, handles all plumbing
- `src/Common/test/IntegrationTests/HttpAssert.cs` — static status code assertion helpers
- `src/Modules/Sales/test/IntegrationTests/Abstractions/SalesIntegrationTestFixture.cs` — module fixture with virtual overrides

### Files to Modify
- `src/Common/test/IntegrationTests/Rtl.Core.IntegrationTests.csproj` — remove InMemory package
- `src/Common/test/IntegrationTests/Abstractions/BaseIntegrationTest.cs` — update to use `IntegrationTestFixture<Program>`
- `src/Common/test/IntegrationTests/Abstractions/IntegrationTestCollection.cs` — update fixture type
- `src/Modules/Sales/test/IntegrationTests/Abstractions/IntegrationTestCollection.cs` — point at new fixture
- `src/Modules/Sales/test/IntegrationTests/Abstractions/SalesIntegrationTestBase.cs` — remove infrastructure, keep Arrange helpers
- All 13 Sales test files — update constructor parameter type from `SalesTestFactory` to `SalesIntegrationTestFixture`

### Files Unchanged
- `src/Modules/Sales/test/IntegrationTests/Abstractions/FakeiSeriesAdapter.cs`
- `src/Modules/Sales/test/IntegrationTests/Abstractions/SalesHttpHelpers.cs`
- All test method bodies (assertions, endpoints, data)

---

### Task 1: Create IntegrationTestFixture<TEntryPoint>

**Files:**
- Create: `src/Common/test/IntegrationTests/IntegrationTestFixture.cs`

- [ ] **Step 1: Write the generic base class**

This replaces `IntegrationTestWebAppFactory` + all 4 `DatabaseProvider` files. It extends `WebApplicationFactory<TEntryPoint>` and implements `IAsyncLifetime`. All configuration is handled internally with virtual extension points.

```csharp
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

    // ── WebApplicationFactory configuration ─────────────────────

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Environment must be Development so Program.cs applies migrations
        builder.UseEnvironment("Development");

        // Database
        builder.UseSetting("ConnectionStrings:Database", GetConnectionString());
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

        // Module-specific service overrides
        builder.ConfigureTestServices(ConfigureTestServices);
    }

    // ── Lifecycle ────────────────────────────────────────────────

    public async Task InitializeAsync()
    {
        // Build the app — triggers Program.cs (migrations, DI, middleware)
        _ = Services;

        // Initialize Respawn after migrations have run
        await InitializeRespawnerAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }

    // Called before each test by the test base class
    public async Task ResetDatabaseAsync()
    {
        if (_respawner is not null)
        {
            await using var connection = new NpgsqlConnection(GetConnectionString());
            await connection.OpenAsync();
            await _respawner.ResetAsync(connection);
        }

        // Seed reference data in a fresh scope
        using var scope = Services.CreateScope();
        await SeedReferenceDataAsync(scope);
    }

    // ── Virtual extension points (override in module fixtures) ──

    // Override to register module-specific fakes (e.g., FakeiSeriesAdapter)
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

    // ── Internal ─────────────────────────────────────────────────

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
```

- [ ] **Step 2: Verify it compiles**

Run: `dotnet build src/Common/test/IntegrationTests/Rtl.Core.IntegrationTests.csproj`
Expected: Build succeeds (new file, no references to it yet)

- [ ] **Step 3: Commit**

```bash
git add src/Common/test/IntegrationTests/IntegrationTestFixture.cs
git commit -m "feat: add generic IntegrationTestFixture<TEntryPoint> base class"
```

---

### Task 2: Create HttpAssert Helper

**Files:**
- Create: `src/Common/test/IntegrationTests/HttpAssert.cs`

- [ ] **Step 1: Write the static helper class**

```csharp
using System.Net;
using Xunit;

namespace Rtl.Core.IntegrationTests;

// Static helpers for asserting HTTP status codes with descriptive failure messages.
// Usage: HttpAssert.IsCreated(response);
public static class HttpAssert
{
    public static void IsOk(HttpResponseMessage response)
        => Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    public static void IsCreated(HttpResponseMessage response)
        => Assert.Equal(HttpStatusCode.Created, response.StatusCode);

    public static void IsNoContent(HttpResponseMessage response)
        => Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

    public static void IsBadRequest(HttpResponseMessage response)
        => Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

    public static void IsNotFound(HttpResponseMessage response)
        => Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

    public static void IsConflict(HttpResponseMessage response)
        => Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

    public static void IsUnauthorized(HttpResponseMessage response)
        => Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

    public static void IsForbidden(HttpResponseMessage response)
        => Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
}
```

- [ ] **Step 2: Verify it compiles**

Run: `dotnet build src/Common/test/IntegrationTests/Rtl.Core.IntegrationTests.csproj`
Expected: Build succeeds

- [ ] **Step 3: Commit**

```bash
git add src/Common/test/IntegrationTests/HttpAssert.cs
git commit -m "feat: add HttpAssert static helper for status code assertions"
```

---

### Task 3: Create SalesIntegrationTestFixture

**Files:**
- Create: `src/Modules/Sales/test/IntegrationTests/Abstractions/SalesIntegrationTestFixture.cs`

- [ ] **Step 1: Write the Sales module fixture**

This replaces `SalesTestFactory`. It overrides the virtual methods from `IntegrationTestFixture<Program>` to register the fake iSeries adapter, specify schemas, and seed reference data.

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Sales.Domain.CustomersCache;
using Modules.Sales.Domain.RetailLocationCache;
using Modules.Sales.Infrastructure.Persistence;
using Rtl.Core.Application.Adapters.ISeries;
using Rtl.Core.Application.Caching;
using Rtl.Core.IntegrationTests;
using RetailLocationCacheEntity = Modules.Sales.Domain.RetailLocationCache.RetailLocationCache;

namespace Modules.Sales.IntegrationTests.Abstractions;

// Sales module integration test fixture.
// Registers FakeiSeriesAdapter, configures Respawn for Sales schemas,
// and seeds retail location + customer cache before each test.
public sealed class SalesIntegrationTestFixture : IntegrationTestFixture<Program>
{
    // Known test data — single source of truth for all Sales tests
    public const int TestHomeCenterNumber = 100;
    public const int TestAuthorizedUserId1 = 1;
    public const int TestAuthorizedUserId2 = 2;

    // Set after seeding — available to tests via the base class
    public Guid TestCustomerId { get; private set; }

    protected override void ConfigureTestServices(IServiceCollection services)
    {
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
            var retailLocation = RetailLocationCacheEntity.CreateHomeCenter(
                TestHomeCenterNumber, "Test HC", "TN", "37801", isActive: true);
            db.Set<RetailLocationCacheEntity>().Add(retailLocation);

            var customerPublicId = Guid.NewGuid();
            db.Set<CustomerCache>().Add(new CustomerCache
            {
                RefPublicId = customerPublicId,
                HomeCenterNumber = TestHomeCenterNumber,
                LifecycleStage = LifecycleStage.Customer,
                DisplayName = "Test Customer",
                FirstName = "Test",
                LastName = "Customer",
                LastSyncedAtUtc = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
        }

        // Read back the customer ID (identity column assigns the PK)
        var customer = await db.Set<CustomerCache>()
            .FirstAsync(customer => customer.HomeCenterNumber == TestHomeCenterNumber);
        TestCustomerId = customer.RefPublicId;
    }
}
```

- [ ] **Step 2: Verify it compiles**

Run: `dotnet build src/Modules/Sales/test/IntegrationTests/Modules.Sales.IntegrationTests.csproj`
Expected: Build succeeds

- [ ] **Step 3: Commit**

```bash
git add src/Modules/Sales/test/IntegrationTests/Abstractions/SalesIntegrationTestFixture.cs
git commit -m "feat: add SalesIntegrationTestFixture with virtual overrides for seeding and fakes"
```

---

### Task 4: Update SalesIntegrationTestBase to Use New Fixture

**Files:**
- Modify: `src/Modules/Sales/test/IntegrationTests/Abstractions/SalesIntegrationTestBase.cs`

- [ ] **Step 1: Remove infrastructure concerns, keep Arrange helpers**

The base class no longer does reference data seeding or creates scopes for cache writes. It receives the fixture and delegates to it. The `TestCustomerId` and `TestHomeCenterNumber` come from the fixture.

Replace the entire file with:

```csharp
using System.Net.Http.Json;
using Bogus;
using Modules.Sales.Application.Packages.GetPackageById;
using Modules.Sales.Application.Packages.UpdatePackageHome;
using Modules.Sales.Domain.Packages.Home;
using Modules.Sales.Presentation.Endpoints.V1.DeliveryAddress;
using Modules.Sales.Presentation.Endpoints.V1.Packages;
using Modules.Sales.Presentation.Endpoints.V1.Sales;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.IntegrationTests.Abstractions;

// Base class for Sales module integration tests.
//
// Provides:
// - Arrange helpers that build up state step by step (ArrangeSaleAsync -> ArrangeSaleWithHomeAsync)
// - State tracking (SaleId, PackageId, etc.)
// - TestCustomerId and TestHomeCenterNumber from the fixture's seeded data
//
// Does NOT handle: DI, database connections, Respawn, cache scopes, encryption keys.
// All of that is invisible — handled by SalesIntegrationTestFixture.
[Collection("SalesIntegration")]
public abstract class SalesIntegrationTestBase(SalesIntegrationTestFixture fixture) : IAsyncLifetime
{
    protected static readonly Faker Faker = new();

    protected readonly SalesIntegrationTestFixture Fixture = fixture;
    protected readonly HttpClient Client = fixture.CreateClient();

    // Reference data from the fixture's SeedReferenceDataAsync
    protected Guid TestCustomerId => Fixture.TestCustomerId;
    protected int TestHomeCenterNumber => SalesIntegrationTestFixture.TestHomeCenterNumber;

    // Populated by Arrange helpers
    protected Guid SaleId { get; private set; }
    protected int SaleNumber { get; private set; }
    protected Guid PackageId { get; private set; }
    protected Guid DeliveryAddressId { get; private set; }

    // ── Lifecycle ────────────────────────────────────────────────

    public async Task InitializeAsync() => await Fixture.ResetDatabaseAsync();

    public Task DisposeAsync()
    {
        Client.Dispose();
        return Task.CompletedTask;
    }

    // ── Arrange helpers ─────────────────────────────────────────

    // Creates a sale. Sets SaleId and SaleNumber.
    protected async Task ArrangeSaleAsync()
    {
        var body = await Client.PostAsync<ApiEnvelope<CreateSaleResponse>>(
            "/api/v1/sales",
            new CreateSaleRequest(TestCustomerId, TestHomeCenterNumber));

        SaleId = body!.Data!.Id;
        SaleNumber = body.Data.SaleNumber;
    }

    // Creates a sale + delivery address. Sets SaleId, SaleNumber, DeliveryAddressId.
    protected async Task ArrangeSaleWithDeliveryAsync()
    {
        await ArrangeSaleAsync();

        var body = await Client.PostAsync<ApiEnvelope<CreateDeliveryAddressResponse>>(
            $"/api/v1/sales/{SaleId}/delivery-address",
            new CreateDeliveryAddressRequest(
                OccupancyType: "Primary Residence",
                IsWithinCityLimits: true,
                AddressLine1: "123 Test St",
                City: "Nashville",
                County: "Davidson",
                State: "TN",
                PostalCode: "37201"));

        DeliveryAddressId = body!.Data!.Id;
    }

    // Creates a sale + delivery address + package. Sets SaleId, SaleNumber, DeliveryAddressId, PackageId.
    protected async Task ArrangeSaleWithPackageAsync(string packageName = "Primary")
    {
        await ArrangeSaleWithDeliveryAsync();

        var body = await Client.PostAsync<ApiEnvelope<CreatePackageResponse>>(
            $"/api/v1/sales/{SaleId}/packages",
            new CreatePackageRequest(packageName));

        PackageId = body!.Data!.Id;
    }

    // Creates sale + delivery + package + baseline home. Full setup for most package tests.
    protected async Task ArrangeSaleWithHomeAsync(
        decimal homeSalePrice = 80_000m,
        decimal homeEstimatedCost = 60_000m,
        decimal homeRetailSalePrice = 80_000m,
        int numberOfFloorSections = 1,
        WheelAndAxlesOption wheelAndAxles = WheelAndAxlesOption.Rent)
    {
        await ArrangeSaleWithPackageAsync();

        await Client.PutAndAssertOkAsync(
            $"/api/v1/packages/{PackageId}/home",
            new UpdatePackageHomeRequest(
                SalePrice: homeSalePrice,
                EstimatedCost: homeEstimatedCost,
                RetailSalePrice: homeRetailSalePrice,
                StockNumber: null,
                HomeType: HomeType.New,
                HomeSourceType: HomeSourceType.Manual,
                ModularType: ModularType.Hud,
                Vendor: "TestVendor",
                Make: "TestMake",
                Model: "TestModel",
                ModelYear: 2026,
                LengthInFeet: 76.0m,
                WidthInFeet: 28.0m,
                Bedrooms: 3,
                Bathrooms: 2.0m,
                SquareFootage: "2128",
                SerialNumbers: ["TEST123AB"],
                BaseCost: homeEstimatedCost * 0.8m,
                OptionsCost: homeEstimatedCost * 0.12m,
                FreightCost: homeEstimatedCost * 0.08m,
                InvoiceCost: homeEstimatedCost,
                NetInvoice: homeEstimatedCost * 0.98m,
                GrossCost: homeEstimatedCost,
                TaxIncludedOnInvoice: 0m,
                NumberOfWheels: 4,
                NumberOfAxles: 2,
                WheelAndAxlesOption: wheelAndAxles,
                NumberOfFloorSections: numberOfFloorSections,
                CarrierFrameDeposit: 500m,
                RebateOnMfgInvoice: 0m,
                ClaytonBuilt: true,
                BuildType: numberOfFloorSections > 1 ? "Double" : "Single",
                InventoryReferenceId: null,
                StateAssociationAndMhiDues: 150m,
                PartnerAssistance: 0m,
                DistanceMiles: 50.0,
                PropertyType: null,
                PurchaseOption: null,
                ListingPrice: null,
                AccountNumber: null,
                DisplayAccountId: null,
                StreetAddress: null,
                City: null,
                State: null,
                ZipCode: null));
    }

    // GET the current package detail using the stored PackageId.
    protected async Task<PackageDetailResponse> GetPackageAsync()
        => await Client.GetPackageAsync(PackageId);
}
```

- [ ] **Step 2: Verify it compiles**

Run: `dotnet build src/Modules/Sales/test/IntegrationTests/Modules.Sales.IntegrationTests.csproj`
Expected: Build fails — test files still reference `SalesTestFactory`. That's expected, we fix them in Task 6.

- [ ] **Step 3: Commit**

```bash
git add src/Modules/Sales/test/IntegrationTests/Abstractions/SalesIntegrationTestBase.cs
git commit -m "refactor: remove infrastructure from SalesIntegrationTestBase, delegate to fixture"
```

---

### Task 5: Update Collection Definitions and Delete Old Files

**Files:**
- Modify: `src/Modules/Sales/test/IntegrationTests/Abstractions/IntegrationTestCollection.cs`
- Modify: `src/Common/test/IntegrationTests/Abstractions/IntegrationTestCollection.cs`
- Modify: `src/Common/test/IntegrationTests/Abstractions/BaseIntegrationTest.cs`
- Modify: `src/Common/test/IntegrationTests/Rtl.Core.IntegrationTests.csproj`
- Delete: `src/Common/test/IntegrationTests/DatabaseProviders/ITestDatabaseProvider.cs`
- Delete: `src/Common/test/IntegrationTests/DatabaseProviders/TestDatabaseProviderFactory.cs`
- Delete: `src/Common/test/IntegrationTests/DatabaseProviders/InMemoryTestDatabaseProvider.cs`
- Delete: `src/Common/test/IntegrationTests/DatabaseProviders/PostgreSqlTestDatabaseProvider.cs`
- Delete: `src/Common/test/IntegrationTests/Abstractions/IntegrationTestWebAppFactory.cs`
- Delete: `src/Modules/Sales/test/IntegrationTests/Abstractions/SalesTestFactory.cs`

- [ ] **Step 1: Update Sales IntegrationTestCollection**

Replace `src/Modules/Sales/test/IntegrationTests/Abstractions/IntegrationTestCollection.cs`:

```csharp
namespace Modules.Sales.IntegrationTests.Abstractions;

[CollectionDefinition("SalesIntegration")]
public sealed class SalesIntegrationTestCollection : ICollectionFixture<SalesIntegrationTestFixture>;
```

- [ ] **Step 2: Update Common IntegrationTestCollection**

Replace `src/Common/test/IntegrationTests/Abstractions/IntegrationTestCollection.cs`:

```csharp
using Rtl.Core.IntegrationTests;

namespace Rtl.Core.IntegrationTests.Abstractions;

[CollectionDefinition(nameof(IntegrationTestCollection))]
public sealed class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture<Program>>;
```

- [ ] **Step 3: Update BaseIntegrationTest**

Replace `src/Common/test/IntegrationTests/Abstractions/BaseIntegrationTest.cs`:

```csharp
using Bogus;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Rtl.Core.IntegrationTests.Abstractions;

[Collection(nameof(IntegrationTestCollection))]
public abstract class BaseIntegrationTest : IAsyncLifetime
{
    protected static readonly Faker Faker = new();

    private readonly IServiceScope _scope;
    protected readonly IntegrationTestFixture<Program> Factory;
    protected readonly ISender Sender;

    protected BaseIntegrationTest(IntegrationTestFixture<Program> factory)
    {
        Factory = factory;
        _scope = factory.Services.CreateScope();
        Sender = _scope.ServiceProvider.GetRequiredService<ISender>();
    }

    protected T GetService<T>() where T : notnull
        => _scope.ServiceProvider.GetRequiredService<T>();

    public async Task InitializeAsync() => await Factory.ResetDatabaseAsync();

    public Task DisposeAsync()
    {
        _scope.Dispose();
        return Task.CompletedTask;
    }
}
```

- [ ] **Step 4: Remove InMemory package from csproj**

In `src/Common/test/IntegrationTests/Rtl.Core.IntegrationTests.csproj`, remove:
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="10.0.2" />
```

- [ ] **Step 5: Delete old files**

```bash
rm src/Common/test/IntegrationTests/DatabaseProviders/ITestDatabaseProvider.cs
rm src/Common/test/IntegrationTests/DatabaseProviders/TestDatabaseProviderFactory.cs
rm src/Common/test/IntegrationTests/DatabaseProviders/InMemoryTestDatabaseProvider.cs
rm src/Common/test/IntegrationTests/DatabaseProviders/PostgreSqlTestDatabaseProvider.cs
rm src/Common/test/IntegrationTests/Abstractions/IntegrationTestWebAppFactory.cs
rm src/Modules/Sales/test/IntegrationTests/Abstractions/SalesTestFactory.cs
```

- [ ] **Step 6: Verify Common project compiles**

Run: `dotnet build src/Common/test/IntegrationTests/Rtl.Core.IntegrationTests.csproj`
Expected: Build succeeds

- [ ] **Step 7: Commit**

```bash
git add -A src/Common/test/IntegrationTests/
git add src/Modules/Sales/test/IntegrationTests/Abstractions/IntegrationTestCollection.cs
git add src/Modules/Sales/test/IntegrationTests/Abstractions/SalesTestFactory.cs
git commit -m "refactor: delete provider abstraction, update collections to use new fixture"
```

---

### Task 6: Update All Test Files

**Files:**
- Modify: All 13 test files in `src/Modules/Sales/test/IntegrationTests/`

The only change: replace `SalesTestFactory` with `SalesIntegrationTestFixture` in constructor parameters.

- [ ] **Step 1: Bulk replace constructor parameter type**

In every test file under `Sales/test/IntegrationTests/`, replace:
- `SalesTestFactory factory` with `SalesIntegrationTestFixture fixture`
- `SalesIntegrationTestBase(factory)` with `SalesIntegrationTestBase(fixture)`

Also replace any direct references to `SalesTestFactory.TestAuthorizedUserId1` with `SalesIntegrationTestFixture.TestAuthorizedUserId1` (and UserId2).

Files to update:
- `Sales/CreateSaleTests.cs`
- `Sales/SaleLifecycleTests.cs`
- `DeliveryAddresses/CreateDeliveryAddressTests.cs`
- `DeliveryAddresses/UpdateDeliveryAddressTests.cs`
- `Packages/UpdatePackageHomeTests.cs`
- `Packages/UpdatePackageLandTests.cs`
- `Packages/UpdatePackageTradeInsTests.cs`
- `Packages/UpdatePackageConcessionsTests.cs`
- `Packages/UpdatePackageDownPaymentTests.cs`
- `Packages/UpdatePackageWarrantyTests.cs`
- `Packages/UpdatePackageInsuranceTests.cs`
- `Packages/UpdatePackageSalesTeamTests.cs`
- `Packages/UpdatePackageTaxTests.cs`
- `Packages/CumulativeGrossProfitTests.cs`

Use bash for bulk replacement:
```bash
cd src/Modules/Sales/test/IntegrationTests
find . -name "*.cs" -exec sed -i \
  -e 's/SalesTestFactory factory/SalesIntegrationTestFixture fixture/g' \
  -e 's/SalesIntegrationTestBase(factory)/SalesIntegrationTestBase(fixture)/g' \
  -e 's/SalesTestFactory\./SalesIntegrationTestFixture./g' \
  {} +
```

- [ ] **Step 2: Verify full solution compiles**

Run: `dotnet build src/Modules/Sales/test/IntegrationTests/Modules.Sales.IntegrationTests.csproj`
Expected: Build succeeds

- [ ] **Step 3: Run all 40 tests**

Run: `dotnet test src/Modules/Sales/test/IntegrationTests/ --verbosity quiet`
Expected: `Passed: 40, Failed: 0`

- [ ] **Step 4: Commit**

```bash
git add src/Modules/Sales/test/IntegrationTests/
git commit -m "refactor: update all test files to use SalesIntegrationTestFixture"
```

---

### Task 7: Run Full Suite and Clean Up

- [ ] **Step 1: Run all integration tests across both modules**

Run SampleSales tests too (they use BaseIntegrationTest which was updated):
```bash
dotnet test src/Modules/SampleSales/test/IntegrationTests/ --verbosity quiet
dotnet test src/Modules/Sales/test/IntegrationTests/ --verbosity quiet
```
Expected: All tests pass in both modules

- [ ] **Step 2: Delete empty DatabaseProviders directory**

```bash
rmdir src/Common/test/IntegrationTests/DatabaseProviders
```

- [ ] **Step 3: Final commit**

```bash
git add -A
git commit -m "chore: clean up empty DatabaseProviders directory"
```
