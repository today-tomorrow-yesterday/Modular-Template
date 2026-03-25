using System.Net;
using System.Net.Http.Json;
using Modules.Sales.IntegrationTests.Abstractions;
using Modules.Sales.Presentation.Endpoints.V1.DeliveryAddress;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.IntegrationTests.DeliveryAddresses;

// POST /api/v1/sales/{saleId}/delivery-address
//
// Tests the CreateDeliveryAddress endpoint.
//
// Tests:
// - Valid request -> 201 Created with delivery address ID
// - Duplicate address -> 409 Conflict (only one per sale)
// - Unknown sale ID -> 404 Not Found
// - Persistence: POST then GET back, verify all fields
public class CreateDeliveryAddressTests(SalesIntegrationTestFixture fixture) : SalesIntegrationTestBase(fixture)
{
    private string Endpoint => $"/api/v1/sales/{SaleId}/delivery-address";

    [Fact]
    public async Task Should_ReturnCreated_WhenRequestIsValid()
    {
        // Arrange
        await ArrangeSaleAsync();

        var request = new CreateDeliveryAddressRequest(
            OccupancyType: "Primary Residence",
            IsWithinCityLimits: true,
            AddressLine1: "5000 Clayton Rd",
            City: "Maryville",
            County: "Blount",
            State: "TN",
            PostalCode: "37801");

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, request);
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<CreateDeliveryAddressResponse>>();

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode); // Should have returned 201 Created
        Assert.NotNull(body?.Data);                                // Should have returned delivery address data
        Assert.NotEqual(Guid.Empty, body.Data.Id);                 // Should have generated a valid delivery address ID
    }

    [Fact]
    public async Task Should_ReturnConflict_WhenAddressAlreadyExists()
    {
        // Arrange
        await ArrangeSaleAsync();

        var request = new CreateDeliveryAddressRequest(
            "Primary Residence", true, "123 Main St", "Maryville", "Blount", "TN", "37801");

        var first = await Client.PostAsJsonAsync(Endpoint, request);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);   // Should have created the first address

        // Act
        var second = await Client.PostAsJsonAsync(Endpoint, request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode); // Should have rejected duplicate address
    }

    [Fact]
    public async Task Should_ReturnNotFound_WhenSaleDoesNotExist()
    {
        // Arrange
        var unknownSaleId = Guid.NewGuid();
        var request = new CreateDeliveryAddressRequest(
            "Primary Residence", true, "123 Main St", "Maryville", "Blount", "TN", "37801");

        // Act
        var response = await Client.PostAsJsonAsync(
            $"/api/v1/sales/{unknownSaleId}/delivery-address", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode); // Should have returned 404 for unknown sale
    }

    [Fact]
    public async Task Should_PersistAllFields_WhenGetAfterCreate()
    {
        // Arrange
        await ArrangeSaleAsync();

        var request = new CreateDeliveryAddressRequest(
            OccupancyType: "Primary Residence",
            IsWithinCityLimits: true,
            AddressLine1: "5000 Clayton Rd",
            City: "Maryville",
            County: "Blount",
            State: "TN",
            PostalCode: "37801");

        // Act
        var createResponse = await Client.PostAsJsonAsync(Endpoint, request);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);   // Should have created the address

        var getResponse = await Client.GetAsync<ApiEnvelope<DeliveryAddressResponse>>(Endpoint);

        // Assert
        Assert.NotNull(getResponse?.Data);                                 // Should have returned the delivery address
        Assert.Equal("Primary Residence", getResponse.Data.OccupancyType); // Should have persisted occupancy type
        Assert.True(getResponse.Data.IsWithinCityLimits);                  // Should have persisted city limits flag
        Assert.Equal("5000 Clayton Rd", getResponse.Data.AddressLine1);    // Should have persisted address line 1
        Assert.Equal("Maryville", getResponse.Data.City);                  // Should have persisted city
        Assert.Equal("Blount", getResponse.Data.County);                   // Should have persisted county
        Assert.Equal("TN", getResponse.Data.State);                        // Should have persisted state
        Assert.Equal("37801", getResponse.Data.PostalCode);                // Should have persisted postal code
    }
}
