using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Sales.Domain.AuthorizedUsersCache;
using Modules.Sales.Domain.CustomersCache;
using Modules.Sales.Infrastructure.Persistence;
using Rtl.Core.Application.Adapters.ISeries;
using Rtl.Core.Application.Caching;
using Rtl.Core.IntegrationTests;
using RetailLocationCacheEntity = Modules.Sales.Domain.RetailLocationCache.RetailLocationCache;

namespace Modules.Sales.Integration.Shared;

// Boots the full application via WebApplicationFactory and configures it for Sales integration testing.
//
// Between each test:
// - Respawn truncates the sales, packages, and cache schemas
// - Reference data is re-seeded: a RetailLocationCache (HC 100), a CustomerCache, and two AuthorizedUserCache entries
//
// The real iSeries adapter is replaced with FakeiSeriesAdapter so tests don't call external systems.
// Test constants (TestHomeCenterNumber, TestAuthorizedUserId1/2, TestCustomerId) are available
// for any test that needs to reference the seeded data.
public abstract class SalesTestFixtureBase : IntegrationTestFixture<Program>
{
    public const int TestHomeCenterNumber = 100;
    public const int TestAuthorizedUserId1 = 1;
    public const int TestAuthorizedUserId2 = 2;

    public Guid TestCustomerId { get; private set; }

    // ── Fixture configuration ──────────────────────────────────────

    protected override string ResolveConnectionString(IConfiguration configuration)
        => configuration["Modules:Sales:ConnectionStrings:Database"]
           ?? configuration.GetConnectionString("Database")
           ?? throw new InvalidOperationException(
               "No Sales database connection string found in configuration.");

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

        var customerPublicId = Guid.NewGuid();

        using (cacheScope.AllowWrites())
        {
            var retailLocation = RetailLocationCacheEntity.CreateHomeCenter(TestHomeCenterNumber, "Test HC", "TN", "37801", isActive: true);
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
                    RefUserId = Guid.NewGuid(),
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
                    RefUserId = Guid.NewGuid(),
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
}
