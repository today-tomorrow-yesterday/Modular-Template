using System.Net;
using System.Net.Http.Json;
using Modules.Sales.IntegrationTests.Abstractions;
using Modules.Sales.Presentation.Endpoints.V1.Sales;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.IntegrationTests.Sales;

// POST /api/v1/sales
//
// Tests the CreateSale endpoint in isolation. No package or delivery address setup needed.
//
// Tests:
// - Valid customer + home center -> 201 Created with sale ID and sale number
// - Unknown customer ID -> error response (Customer.NotFound)
// - Unknown home center number -> error response (RetailLocation.NotFound)
public class CreateSaleTests(SalesIntegrationTestFixture fixture) : SalesIntegrationTestBase(fixture)
{
    private const string Endpoint = "/api/v1/sales";

    [Fact]
    public async Task Should_ReturnCreated_WhenRequestIsValid()
    {
        // Arrange
        var request = new CreateSaleRequest(TestCustomerId, TestHomeCenterNumber);

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, request);
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<CreateSaleResponse>>();

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);       // Should have returned 201 Created
        Assert.NotNull(body?.Data);                                       // Should have returned sale data
        Assert.NotEqual(Guid.Empty, body.Data.Id);                       // Should have generated a valid sale ID
        Assert.True(body.Data.SaleNumber > 0);                           // Should have assigned a sale number
    }

    [Fact]
    public async Task Should_ReturnProblem_WhenCustomerNotFound()
    {
        // Arrange
        var request = new CreateSaleRequest(Guid.NewGuid(), TestHomeCenterNumber);

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, request);
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<CreateSaleResponse>>();

        // Assert
        Assert.NotNull(body);                                             // Should have returned a response body
        Assert.False(body.IsSuccess);                                     // Should have returned a failure for unknown customer
    }

    [Fact]
    public async Task Should_ReturnProblem_WhenHomeCenterNotFound()
    {
        // Arrange
        var request = new CreateSaleRequest(TestCustomerId, 99999);

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, request);
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<CreateSaleResponse>>();

        // Assert
        Assert.NotNull(body);                                             // Should have returned a response body
        Assert.False(body.IsSuccess);                                     // Should have returned a failure for unknown home center
    }
}
