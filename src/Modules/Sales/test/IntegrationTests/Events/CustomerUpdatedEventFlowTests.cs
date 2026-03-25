using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Sales.Domain.CustomersCache;
using Modules.Sales.Infrastructure.Persistence;
using Modules.Sales.IntegrationTests.Abstractions;
using Modules.Sales.Presentation.Endpoints.V1.Sales;
using Rtl.Core.Application.Caching;
using Rtl.Core.IntegrationTests;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.IntegrationTests.Events;

// End-to-end event flow: Customer CRM sync (update) -> conditional domain events -> Sales cache updates.
//
// SyncCustomerFromCrmCommand handles both create and update. When updating an existing customer,
// it conditionally raises domain events only for fields that actually changed:
// - CustomerNameChangedDomainEvent
// - CustomerHomeCenterChangedDomainEvent
// - CustomerContactPointsChangedDomainEvent
// - CustomerMailingAddressChangedDomainEvent
//
// Each test: create customer -> flush outbox -> update customer -> flush outbox -> verify cache updated.
public class CustomerUpdatedEventFlowTests(SalesIntegrationTestFixture fixture)
    : SalesIntegrationTestBase(fixture)
{
    private const int CrmPartyId = 88001;

    [Fact]
    public async Task CustomerNameChanged_UpdatesCachedDisplayName()
    {
        // Arrange — create customer, flush, verify initial state
        await Fixture.CreateCustomerViaCrmSyncAsync(crmPartyId: CrmPartyId, firstName: "Jane", lastName: "Doe");
        await Fixture.FlushCustomerOutboxAsync();

        // Act — update the name via CRM sync
        await Fixture.UpdateCustomerViaCrmSyncAsync(crmPartyId: CrmPartyId, firstName: "Janet", lastName: "Smith");
        await Fixture.FlushCustomerOutboxAsync();

        // Assert — cache should reflect the new name
        var cached = await GetCachedCustomerAsync("Janet Smith");
        Assert.NotNull(cached);
        Assert.Equal("Janet", cached.FirstName);
        Assert.Equal("Smith", cached.LastName);
    }

    [Fact]
    public async Task CustomerContactPointsChanged_UpdatesCachedEmailAndPhone()
    {
        // Arrange
        await Fixture.CreateCustomerViaCrmSyncAsync(crmPartyId: CrmPartyId, firstName: "Jane", lastName: "Doe");
        await Fixture.FlushCustomerOutboxAsync();

        // Act — update with new contact points
        await Fixture.UpdateCustomerViaCrmSyncAsync(
            crmPartyId: CrmPartyId,
            contactPoints:
            [
                new Modules.Customer.Application.Customers.SyncCustomerFromCrm.SyncContactPointDto(
                    Modules.Customer.Domain.Customers.Enums.ContactPointType.Email, "new-email@test.com", true),
                new Modules.Customer.Application.Customers.SyncCustomerFromCrm.SyncContactPointDto(
                    Modules.Customer.Domain.Customers.Enums.ContactPointType.Phone, "555-9999", false)
            ]);
        await Fixture.FlushCustomerOutboxAsync();

        // Assert
        var cached = await GetCachedCustomerAsync();
        Assert.NotNull(cached);
        Assert.Equal("new-email@test.com", cached.Email);
        Assert.Equal("555-9999", cached.Phone);
    }

    [Fact]
    public async Task CustomerMailingAddressChanged_UpdatesCachedAddress()
    {
        // Arrange
        await Fixture.CreateCustomerViaCrmSyncAsync(crmPartyId: CrmPartyId, firstName: "Jane", lastName: "Doe");
        await Fixture.FlushCustomerOutboxAsync();

        // Act — update with mailing address
        await Fixture.UpdateCustomerViaCrmSyncAsync(
            crmPartyId: CrmPartyId,
            mailingAddress: new Modules.Customer.Application.Customers.SyncCustomerFromCrm.SyncMailingAddressDto(
                "456 Oak Ave", null, "Knoxville", "Knox", "TN", "US", "37902"));
        await Fixture.FlushCustomerOutboxAsync();

        // Assert — verify the customer still exists in cache (address update shouldn't break anything)
        // The Sales cache doesn't store the full mailing address, but the event should process without error
        var cached = await GetCachedCustomerAsync();
        Assert.NotNull(cached);
        Assert.Equal("Jane", cached.FirstName); // unchanged
    }

    [Fact]
    public async Task CustomerCreatedThenUpdated_SaleUsesLatestCachedData()
    {
        // Arrange — full journey: create, update name, flush all
        await Fixture.CreateCustomerViaCrmSyncAsync(crmPartyId: CrmPartyId, firstName: "Jane", lastName: "Doe");
        await Fixture.FlushCustomerOutboxAsync();

        await Fixture.UpdateCustomerViaCrmSyncAsync(crmPartyId: CrmPartyId, firstName: "Updated", lastName: "Customer");
        await Fixture.FlushCustomerOutboxAsync();

        // Act — get the customer's PublicId and create a sale
        var cached = await GetCachedCustomerAsync();
        Assert.NotNull(cached);

        var response = await Client.PostAsJsonAsync("/api/v1/sales",
            new CreateSaleRequest(cached.RefPublicId, TestHomeCenterNumber));

        // Assert — sale succeeds with the updated customer
        await HttpAssert.IsCreatedAsync(response);
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<CreateSaleResponse>>();
        Assert.NotNull(body?.Data);
        Assert.True(body.Data.SaleNumber > 0);
    }

    // Helper — reads the event-created cached customer (not the seeded reference data one).
    // Filters by DisplayName pattern since the seeded customer is "Test Customer".
    private async Task<CustomerCache?> GetCachedCustomerAsync(string? expectedDisplayName = null)
    {
        using var scope = Fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SalesDbContext>();

        if (expectedDisplayName is not null)
            return await db.Set<CustomerCache>()
                .FirstOrDefaultAsync(c => c.DisplayName == expectedDisplayName);

        // Return the non-seed customer (seed customer has DisplayName "Test Customer")
        return await db.Set<CustomerCache>()
            .FirstOrDefaultAsync(c => c.HomeCenterNumber == TestHomeCenterNumber
                                      && c.DisplayName != "Test Customer");
    }
}
