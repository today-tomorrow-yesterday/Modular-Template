using System.Net;
using System.Net.Http.Json;
using Modules.Sales.IntegrationTests.Abstractions;
using Modules.Sales.Presentation.Endpoints.V1.Sales;
using Rtl.Core.IntegrationTests;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.IntegrationTests.Events;

// End-to-end event flow: Customer module -> domain event -> outbox -> event bus -> Sales cache.
//
// Journey: Create Customer (CRM sync) -> Flush Outbox -> Create Sale with cached customer
//
// Proves the pipeline works by creating a customer via the Customer module, flushing the
// outbox, then using the cached customer in a real Sales business operation (CreateSale).
// If the event pipeline failed at any step, CreateSale returns 404 Customer.NotFound.
public class CustomerCreatedEventFlowTests(SalesIntegrationTestFixture fixture)
    : SalesIntegrationTestBase(fixture)
{
    private const string SalesEndpoint = "/api/v1/sales";

    [Fact]
    public async Task CustomerCreated_EventFlow_SaleCanBeCreatedWithCachedCustomer()
    {
        // Arrange — create a customer via CRM sync and flush the outbox
        var customerPublicId = await Fixture.CreateCustomerViaCrmSyncAsync(
            crmPartyId: 99001, firstName: "Jane", lastName: "Doe");

        await Fixture.FlushCustomerOutboxAsync();

        // Act — create a sale using the event-populated customer cache
        var response = await Client.PostAsJsonAsync(SalesEndpoint,
            new CreateSaleRequest(customerPublicId, TestHomeCenterNumber));

        // Assert
        await HttpAssert.IsCreatedAsync(response);                       // Should have returned 201 Created
        var body = await response.Content
            .ReadFromJsonAsync<ApiEnvelope<CreateSaleResponse>>();
        Assert.NotNull(body?.Data);                                      // Should have returned sale data
        Assert.NotEqual(Guid.Empty, body.Data.Id);                       // Should have generated a valid sale ID
        Assert.True(body.Data.SaleNumber > 0);                           // Should have assigned a sale number
    }
}
