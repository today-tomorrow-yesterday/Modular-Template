using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Sales.Domain.CustomersCache;
using Modules.Sales.EventConsumerTests.Abstractions;
using Modules.Sales.Infrastructure.Persistence;
using Modules.Sales.Presentation.Endpoints.V1.Sales;
using Rtl.Core.IntegrationTests;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.EventConsumerTests.Customer;

// Producer:  Customer module (SyncCustomerFromCrmCommand)
// Event:     CustomerCreatedIntegrationEvent
// Consumer:  Sales/Presentation/IntegrationEvents/Customer/CustomerCreatedIntegrationEventHandler
// Effect:    Upserts cache.customers — enables CreateSale to find the customer
//
// Journey: Create Customer (CRM sync) → Flush Outbox → Create Sale with cached customer
//
// If the event pipeline failed at any step, CreateSale returns 404 Customer.NotFound.
public class CustomerCreatedEventFlowTests(EventConsumerTestFixture fixture)
    : EventConsumerTestBase(fixture)
{
    [Fact]
    public async Task CustomerCreated_EventFlow_SaleCanBeCreatedWithCachedCustomer()
    {
        // Arrange — create a customer via CRM sync and flush the outbox
        await CustomerEventHelpers.CreateCustomerViaCrmSyncAsync(Fixture);
        await CustomerEventHelpers.PublishEventsFromOutboxAsync(Fixture);

        // Get the customer's PublicId from the Sales cache (consumer side, not producer)
        using var scope = Fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SalesDbContext>();
        var cached = await db.Set<CustomerCache>()
            .FirstOrDefaultAsync(c => c.DisplayName == "Jane Doe");

        Assert.NotNull(cached);

        // Act — create a sale using the event-populated customer cache
        var response = await Client.PostAsJsonAsync("/api/v1/sales",
            new CreateSaleRequest(cached.RefPublicId, TestHomeCenterNumber));

        // Assert
        await HttpAssert.IsCreatedAsync(response);
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<CreateSaleResponse>>();
        Assert.NotNull(body?.Data);
        Assert.NotEqual(Guid.Empty, body.Data.Id);
        Assert.True(body.Data.SaleNumber > 0);
    }
}
