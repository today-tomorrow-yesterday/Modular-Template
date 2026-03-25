using System.Net;
using System.Net.Http.Json;
using Modules.Sales.EndpointTests.Abstractions;
using Modules.Sales.Presentation.Endpoints.V1.Sales;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.EndpointTests.Sales;

// GET /api/v1/sales/{saleId}
//
// Tests:
// - Valid sale ID -> 200 OK with sale details (Id, SaleNumber, status, customer, retail location)
// - Unknown sale ID -> 404 Not Found
public class GetSaleByIdTests(SalesEndpointTestFixture fixture) : SalesEndpointTestBase(fixture)
{
    [Fact]
    public async Task Should_ReturnSale_WhenSaleExists()
    {
        // Arrange
        await ArrangeSaleAsync();

        // Act
        var response = await Client.GetAsync($"/api/v1/sales/{SaleId}");
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<GetSaleByIdResponse>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);          // Should have returned 200 OK
        Assert.NotNull(body?.Data);                                     // Should have returned sale data
        Assert.Equal(SaleId, body.Data.Id);                             // Should match the created sale ID
        Assert.Equal(SaleNumber, body.Data.SaleNumber);                 // Should match the created sale number
        Assert.Equal(TestCustomerId, body.Data.CustomerId);             // Should match the customer ID
        Assert.NotNull(body.Data.RetailLocation);                       // Should include retail location
        Assert.Equal(TestHomeCenterNumber, body.Data.RetailLocation.HomeCenterNumber); // Should match home center
        Assert.NotNull(body.Data.Customer);                             // Should include customer details
        Assert.Equal("Test", body.Data.Customer.FirstName);             // Should match seeded customer first name
        Assert.Equal("Customer", body.Data.Customer.LastName);          // Should match seeded customer last name
    }

    [Fact]
    public async Task Should_ReturnNotFound_WhenSaleDoesNotExist()
    {
        // Arrange
        var unknownSaleId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/v1/sales/{unknownSaleId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);    // Should have returned 404 Not Found
    }
}
