using System.Net;
using System.Net.Http.Json;
using Modules.Sales.EndpointTests.Abstractions;
using Modules.Sales.Presentation.Endpoints.V1.DeliveryAddress;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.EndpointTests.DeliveryAddresses;

// GET /api/v1/sales/{saleId}/delivery-address
//
// Tests:
// - Sale with delivery address -> 200 OK with address fields
// - Sale without delivery address -> 404 Not Found
public class GetDeliveryAddressTests(SalesEndpointTestFixture fixture) : SalesEndpointTestBase(fixture)
{
    [Fact]
    public async Task Should_ReturnAddress_WhenAddressExists()
    {
        // Arrange
        await ArrangeSaleWithDeliveryAsync();

        // Act
        var response = await Client.GetAsync($"/api/v1/sales/{SaleId}/delivery-address");
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<DeliveryAddressResponse>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);                  // Should have returned 200 OK
        Assert.NotNull(body?.Data);                                             // Should have returned address data
        Assert.Equal("Primary Residence", body.Data.OccupancyType);            // Should match occupancy type
        Assert.True(body.Data.IsWithinCityLimits);                              // Should match city limits flag
        Assert.Equal("123 Test St", body.Data.AddressLine1);                    // Should match address line 1
        Assert.Equal("Nashville", body.Data.City);                              // Should match city
        Assert.Equal("Davidson", body.Data.County);                             // Should match county
        Assert.Equal("TN", body.Data.State);                                    // Should match state
        Assert.Equal("37201", body.Data.PostalCode);                            // Should match postal code
    }

    [Fact]
    public async Task Should_ReturnNotFound_WhenNoAddressExists()
    {
        // Arrange
        await ArrangeSaleAsync();

        // Act
        var response = await Client.GetAsync($"/api/v1/sales/{SaleId}/delivery-address");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);            // Should have returned 404 Not Found
    }
}
