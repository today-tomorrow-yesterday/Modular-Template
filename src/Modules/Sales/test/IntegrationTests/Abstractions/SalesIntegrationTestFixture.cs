using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Modules.Sales.Domain.AuthorizedUsersCache;
using Modules.Sales.Domain.CustomersCache;
using Modules.Sales.Infrastructure.Persistence;
using Npgsql;
using Quartz;
using Respawn;
using Respawn.Graph;
using Rtl.Core.Application.Adapters.ISeries;
using Rtl.Core.Application.Caching;
using Rtl.Core.IntegrationTests;
using RetailLocationCacheEntity = Modules.Sales.Domain.RetailLocationCache.RetailLocationCache;

namespace Modules.Sales.IntegrationTests.Abstractions;

public class SalesIntegrationTestFixture : IntegrationTestFixture<Program>
{
    public const int TestHomeCenterNumber = 100;
    public const int TestAuthorizedUserId1 = 1;
    public const int TestAuthorizedUserId2 = 2;

    private const string CustomerDbConnectionString =
        "Host=localhost;Database=customer_dev;Username=postgres;Password=postgres";

    private Respawner? _customerRespawner;

    public Guid TestCustomerId { get; private set; }

    // ── Event flow helpers ─────────────────────────────────────────

    /// <summary>
    /// Flushes the Customer module outbox by triggering its Quartz job.
    /// After this returns, all pending domain events have been processed
    /// and integration events dispatched via the in-memory bus.
    /// </summary>
    public async Task FlushCustomerOutboxAsync()
    {
        var schedulerFactory = Services.GetRequiredService<ISchedulerFactory>();
        var scheduler = await schedulerFactory.GetScheduler();
        var jobKey = new JobKey("Modules.Customer.Infrastructure.Outbox.ProcessOutboxJob");
        await scheduler.TriggerJob(jobKey);

        // Poll until the outbox is empty (max 5 seconds)
        using var conn = new NpgsqlConnection(CustomerDbConnectionString);
        await conn.OpenAsync();
        for (var i = 0; i < 50; i++)
        {
            await Task.Delay(100);
            await using var cmd = new NpgsqlCommand(
                "SELECT count(*) FROM messaging.outbox_messages WHERE processed_on_utc IS NULL", conn);
            var pending = (long)(await cmd.ExecuteScalarAsync())!;
            if (pending == 0) return;
        }
    }

    // ── Cross-module helpers ───────────────────────────────────────

    // Creates a customer via the Customer module's CRM sync command (no HTTP endpoint exists
    // for customer creation in the Sales module — customers arrive via integration events).
    // Returns the customer's PublicId for use in CreateSale requests.
    public async Task<Guid> CreateCustomerViaCrmSyncAsync(
        int crmPartyId = 99001,
        string firstName = "Jane",
        string lastName = "Doe")
    {
        using var scope = Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<MediatR.ISender>();
        var command = new Modules.Customer.Application.Customers.SyncCustomerFromCrm.SyncCustomerFromCrmCommand(
            CrmPartyId: crmPartyId,
            HomeCenterNumber: TestHomeCenterNumber,
            LifecycleStage: Modules.Customer.Domain.Customers.Enums.LifecycleStage.Customer,
            FirstName: firstName,
            MiddleName: null,
            LastName: lastName,
            NameExtension: null,
            DateOfBirth: new DateOnly(1990, 1, 15),
            SalesAssignments: [],
            ContactPoints:
            [
                new Modules.Customer.Application.Customers.SyncCustomerFromCrm.SyncContactPointDto(
                    Modules.Customer.Domain.Customers.Enums.ContactPointType.Email, $"{firstName.ToLower()}@test.com", true),
                new Modules.Customer.Application.Customers.SyncCustomerFromCrm.SyncContactPointDto(
                    Modules.Customer.Domain.Customers.Enums.ContactPointType.Phone, "555-0100", false)
            ],
            Identifiers:
            [
                new Modules.Customer.Application.Customers.SyncCustomerFromCrm.SyncIdentifierDto(
                    Modules.Customer.Domain.Customers.Enums.IdentifierType.CrmPartyId, crmPartyId.ToString())
            ],
            MailingAddress: null,
            SalesforceUrl: null,
            CreatedOn: DateTimeOffset.UtcNow,
            LastModifiedOn: DateTimeOffset.UtcNow);

        var result = await sender.Send(command);
        if (result.IsFailure)
            throw new InvalidOperationException($"CreateCustomerViaCrmSync failed: {result.Error}");

        // Read back the PublicId from the Customer DB
        var customerDb = scope.ServiceProvider
            .GetRequiredService<Modules.Customer.Infrastructure.Persistence.CustomerDbContext>();
        var customer = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
            .FirstAsync(customerDb.Set<Modules.Customer.Domain.Customers.Entities.Customer>(),
                customer => customer.HomeCenterNumber == TestHomeCenterNumber);

        return customer.PublicId;
    }

    // Updates an existing customer via CRM sync (same command, triggers conditional domain events).
    // The CrmPartyId must match a previously created customer.
    public async Task UpdateCustomerViaCrmSyncAsync(
        int crmPartyId,
        int? homeCenterNumber = null,
        string? firstName = null,
        string? lastName = null,
        Modules.Customer.Application.Customers.SyncCustomerFromCrm.SyncContactPointDto[]? contactPoints = null,
        Modules.Customer.Application.Customers.SyncCustomerFromCrm.SyncMailingAddressDto? mailingAddress = null)
    {
        using var scope = Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<MediatR.ISender>();
        var command = new Modules.Customer.Application.Customers.SyncCustomerFromCrm.SyncCustomerFromCrmCommand(
            CrmPartyId: crmPartyId,
            HomeCenterNumber: homeCenterNumber ?? TestHomeCenterNumber,
            LifecycleStage: Modules.Customer.Domain.Customers.Enums.LifecycleStage.Customer,
            FirstName: firstName ?? "Jane",
            MiddleName: null,
            LastName: lastName ?? "Doe",
            NameExtension: null,
            DateOfBirth: new DateOnly(1990, 1, 15),
            SalesAssignments: [],
            ContactPoints: contactPoints ??
            [
                new Modules.Customer.Application.Customers.SyncCustomerFromCrm.SyncContactPointDto(
                    Modules.Customer.Domain.Customers.Enums.ContactPointType.Email, $"{(firstName ?? "jane").ToLower()}@test.com", true),
                new Modules.Customer.Application.Customers.SyncCustomerFromCrm.SyncContactPointDto(
                    Modules.Customer.Domain.Customers.Enums.ContactPointType.Phone, "555-0100", false)
            ],
            Identifiers:
            [
                new Modules.Customer.Application.Customers.SyncCustomerFromCrm.SyncIdentifierDto(
                    Modules.Customer.Domain.Customers.Enums.IdentifierType.CrmPartyId, crmPartyId.ToString())
            ],
            MailingAddress: mailingAddress,
            SalesforceUrl: null,
            CreatedOn: DateTimeOffset.UtcNow,
            LastModifiedOn: DateTimeOffset.UtcNow);

        var result = await sender.Send(command);
        if (result.IsFailure)
            throw new InvalidOperationException($"UpdateCustomerViaCrmSync failed: {result.Error}");
    }

    // ── Cache seeding helpers ──────────────────────────────────────

    /// <summary>
    /// Seeds a LandParcelCache entry for tests that need HomeCenterOwnedLand lookup.
    /// </summary>
    public async Task SeedLandParcelCacheAsync(string stockNumber, decimal landCost)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SalesDbContext>();
        var cacheWriteScope = scope.ServiceProvider.GetRequiredService<ICacheWriteScope>();

        using (cacheWriteScope.AllowWrites())
        {
            db.Set<Modules.Sales.Domain.InventoryCache.LandParcelCache>().Add(
                new Modules.Sales.Domain.InventoryCache.LandParcelCache
                {
                    RefLandParcelId = Random.Shared.Next(9000, 99999),
                    RefHomeCenterNumber = TestHomeCenterNumber,
                    RefStockNumber = stockNumber,
                    LandCost = landCost,
                    LastSyncedAtUtc = DateTime.UtcNow
                });
            await db.SaveChangesAsync();
        }
    }

    // ── Fixture configuration ──────────────────────────────────────

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        services.AddSingleton<IiSeriesAdapter, FakeiSeriesAdapter>();
    }

    protected override string[] GetSchemasToInclude()
        => ["sales", "packages", "cache"];

    public override async Task ResetDatabaseAsync()
    {
        await base.ResetDatabaseAsync();
        await ResetCustomerDatabaseAsync();
    }

    protected override async Task SeedReferenceDataAsync(IServiceScope scope)
    {
        var db = scope.ServiceProvider.GetRequiredService<SalesDbContext>();
        var cacheScope = scope.ServiceProvider.GetRequiredService<ICacheWriteScope>();

        var customerPublicId = Guid.NewGuid();

        using (cacheScope.AllowWrites())
        {
            var retailLocation = RetailLocationCacheEntity.CreateHomeCenter(
                TestHomeCenterNumber, "Test HC", "TN", "37801", isActive: true);
            db.Set<RetailLocationCacheEntity>().Add(retailLocation);
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

            db.Set<AuthorizedUserCache>().AddRange(
                new AuthorizedUserCache
                {
                    Id = TestAuthorizedUserId1,
                    RefUserId = TestAuthorizedUserId1,
                    FederatedId = "fed-001",
                    EmployeeNumber = 1001,
                    FirstName = "Alice",
                    LastName = "Sales",
                    DisplayName = "Alice Sales",
                    EmailAddress = "alice@test.com",
                    IsActive = true,
                    IsRetired = false,
                    AuthorizedHomeCenters = [TestHomeCenterNumber],
                    LastSyncedAtUtc = DateTime.UtcNow
                },
                new AuthorizedUserCache
                {
                    Id = TestAuthorizedUserId2,
                    RefUserId = TestAuthorizedUserId2,
                    FederatedId = "fed-002",
                    EmployeeNumber = 1002,
                    FirstName = "Bob",
                    LastName = "Sales",
                    DisplayName = "Bob Sales",
                    EmailAddress = "bob@test.com",
                    IsActive = true,
                    IsRetired = false,
                    AuthorizedHomeCenters = [TestHomeCenterNumber],
                    LastSyncedAtUtc = DateTime.UtcNow
                });

            await db.SaveChangesAsync();
        }

        TestCustomerId = customerPublicId;
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
