using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Customer.Application.Customers.SyncCustomerFromCrm;
using Modules.Customer.Domain.Customers.Enums;
using Modules.Sales.Domain.CustomersCache;
using Modules.Sales.EventConsumerTests.Abstractions;
using Modules.Sales.Infrastructure.Persistence;
using Modules.Sales.Presentation.Endpoints.V1.Sales;
using Rtl.Core.IntegrationTests;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.EventConsumerTests.Customer;

// End-to-end event flow: Customer CRM sync (update) -> conditional domain events -> Sales cache updates.
//
// SyncCustomerFromCrmCommand handles both create and update. When updating, it conditionally
// raises domain events only for fields that actually changed. Each test verifies a specific
// update path flows through to the Sales cache.
public class CustomerUpdatedEventFlowTests(EventConsumerTestFixture fixture)
    : EventConsumerTestBase(fixture)
{
    private const int CrmCustomerId = 88001;

    [Fact]
    public async Task CustomerNameChanged_UpdatesCachedDisplayName()
    {
        // Arrange — create customer, flush
        await CustomerEventHelpers.CreateCustomerViaCrmSyncAsync(
            Fixture, crmCustomerId: CrmCustomerId, firstName: "Jane", lastName: "Doe");
        await CustomerEventHelpers.PublishEventsFromOutboxAsync(Fixture);

        // Act — update the name
        await CustomerEventHelpers.UpdateCustomerViaCrmSyncAsync(
            Fixture, crmCustomerId: CrmCustomerId, firstName: "Janet", lastName: "Smith");
        await CustomerEventHelpers.PublishEventsFromOutboxAsync(Fixture);

        // Assert
        var cached = await GetCachedCustomerAsync("Janet Smith");
        Assert.NotNull(cached);
        Assert.Equal("Janet", cached.FirstName);
        Assert.Equal("Smith", cached.LastName);
    }

    [Fact]
    public async Task CustomerContactPointsChanged_UpdatesCachedEmailAndPhone()
    {
        // Arrange
        await CustomerEventHelpers.CreateCustomerViaCrmSyncAsync(
            Fixture, crmCustomerId: CrmCustomerId, firstName: "Jane", lastName: "Doe");
        await CustomerEventHelpers.PublishEventsFromOutboxAsync(Fixture);

        // Act — update contact points
        await CustomerEventHelpers.UpdateCustomerViaCrmSyncAsync(
            Fixture,
            crmCustomerId: CrmCustomerId,
            contactPoints:
            [
                new SyncContactPointDto(ContactPointType.Email, "new-email@test.com", true),
                new SyncContactPointDto(ContactPointType.Phone, "555-9999", false)
            ]);
        await CustomerEventHelpers.PublishEventsFromOutboxAsync(Fixture);

        // Assert
        var cached = await GetCachedCustomerAsync();
        Assert.NotNull(cached);
        Assert.Equal("new-email@test.com", cached.Email);
        Assert.Equal("555-9999", cached.Phone);
    }

    [Fact]
    public async Task CustomerMailingAddressChanged_EventProcessesWithoutError()
    {
        // Arrange
        await CustomerEventHelpers.CreateCustomerViaCrmSyncAsync(
            Fixture, crmCustomerId: CrmCustomerId, firstName: "Jane", lastName: "Doe");
        await CustomerEventHelpers.PublishEventsFromOutboxAsync(Fixture);

        // Act — update with mailing address
        await CustomerEventHelpers.UpdateCustomerViaCrmSyncAsync(
            Fixture,
            crmCustomerId: CrmCustomerId,
            mailingAddress: new SyncMailingAddressDto(
                "456 Oak Ave", null, "Knoxville", "Knox", "TN", "US", "37902"));
        await CustomerEventHelpers.PublishEventsFromOutboxAsync(Fixture);

        // Assert — customer still in cache (address event processed without error)
        var cached = await GetCachedCustomerAsync();
        Assert.NotNull(cached);
        Assert.Equal("Jane", cached.FirstName);
    }

    [Fact]
    public async Task CustomerCreatedThenUpdated_SaleUsesLatestCachedData()
    {
        // Arrange — create, flush, update name, flush
        await CustomerEventHelpers.CreateCustomerViaCrmSyncAsync(
            Fixture, crmCustomerId: CrmCustomerId, firstName: "Jane", lastName: "Doe");
        await CustomerEventHelpers.PublishEventsFromOutboxAsync(Fixture);

        await CustomerEventHelpers.UpdateCustomerViaCrmSyncAsync(
            Fixture, crmCustomerId: CrmCustomerId, firstName: "Updated", lastName: "Customer");
        await CustomerEventHelpers.PublishEventsFromOutboxAsync(Fixture);

        // Act — create a sale using the updated customer
        var cached = await GetCachedCustomerAsync("Updated Customer");
        Assert.NotNull(cached);

        var response = await Client.PostAsJsonAsync("/api/v1/sales",
            new CreateSaleRequest(cached.RefPublicId, TestHomeCenterNumber));

        // Assert
        await HttpAssert.IsCreatedAsync(response);
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<CreateSaleResponse>>();
        Assert.NotNull(body?.Data);
        Assert.True(body.Data.SaleNumber > 0);
    }

    [Fact]
    public async Task CustomerHomeCenterChanged_UpdatesCachedHomeCenterNumber()
    {
        // Arrange
        await CustomerEventHelpers.CreateCustomerViaCrmSyncAsync(
            Fixture, crmCustomerId: CrmCustomerId, firstName: "Jane", lastName: "Doe");
        await CustomerEventHelpers.PublishEventsFromOutboxAsync(Fixture);

        // Act — update home center from 100 to 200
        await CustomerEventHelpers.UpdateCustomerViaCrmSyncAsync(
            Fixture, crmCustomerId: CrmCustomerId, homeCenterNumber: 200);
        await CustomerEventHelpers.PublishEventsFromOutboxAsync(Fixture);

        // Assert — cache should reflect the new home center
        var cached = await GetCachedCustomerAsync("Jane Doe");
        Assert.NotNull(cached);
        Assert.Equal(200, cached.HomeCenterNumber);
    }

    [Fact]
    public async Task CustomerSalesAssignmentsChanged_UpdatesCachedSalesPerson()
    {
        // Arrange
        await CustomerEventHelpers.CreateCustomerViaCrmSyncAsync(
            Fixture, crmCustomerId: CrmCustomerId, firstName: "Jane", lastName: "Doe");
        await CustomerEventHelpers.PublishEventsFromOutboxAsync(Fixture);

        // Act — update with a primary sales assignment
        await CustomerEventHelpers.UpdateCustomerViaCrmSyncAsync(
            Fixture,
            crmCustomerId: CrmCustomerId,
            salesAssignments:
            [
                new SyncSalesAssignmentDto(
                    SalesAssignmentRole.Primary,
                    new SyncSalesPersonDto(
                        Id: "SP-001",
                        Email: "alice@sales.com",
                        Username: "alice.sales",
                        FirstName: "Alice",
                        LastName: "Sales",
                        LotNumber: 100,
                        FederatedId: "fed-alice-001"))
            ]);
        await CustomerEventHelpers.PublishEventsFromOutboxAsync(Fixture);

        // Assert — cache should have the primary sales person
        var cached = await GetCachedCustomerAsync("Jane Doe");
        Assert.NotNull(cached);
        Assert.Equal("fed-alice-001", cached.PrimarySalesPersonFederatedId);
        Assert.Equal("Alice", cached.PrimarySalesPersonFirstName);
        Assert.Equal("Sales", cached.PrimarySalesPersonLastName);
    }

    // Reads the event-created cached customer (not the seeded "Test Customer").
    private async Task<CustomerCache?> GetCachedCustomerAsync(string? expectedDisplayName = null)
    {
        using var scope = Fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SalesDbContext>();

        if (expectedDisplayName is not null)
            return await db.Set<CustomerCache>()
                .FirstOrDefaultAsync(c => c.DisplayName == expectedDisplayName);

        return await db.Set<CustomerCache>()
            .FirstOrDefaultAsync(c => c.HomeCenterNumber == TestHomeCenterNumber
                                      && c.DisplayName != "Test Customer");
    }
}
