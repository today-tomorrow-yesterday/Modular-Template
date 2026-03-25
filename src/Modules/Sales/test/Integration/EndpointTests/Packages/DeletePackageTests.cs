using System.Net;
using Modules.Sales.EndpointTests.Abstractions;
using Modules.Sales.Presentation.Endpoints.V1.Packages;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.EndpointTests.Packages;

// DELETE /api/v1/packages/{packageId}
//
// Tests:
// - Existing non-primary package -> 200 OK, package no longer retrievable
// - Unknown package ID -> 404 Not Found
public class DeletePackageTests(SalesEndpointTestFixture fixture) : SalesEndpointTestBase(fixture)
{
    [Fact]
    public async Task Should_DeletePackage_WhenPackageExists()
    {
        // Arrange — create sale + delivery + primary package, then add a second (non-primary) package
        await ArrangeSaleWithPackageAsync("Primary");

        var secondBody = await Client.PostAsync<ApiEnvelope<CreatePackageResponse>>(
            $"/api/v1/sales/{SaleId}/packages",
            new CreatePackageRequest("Secondary"));
        var secondPackageId = secondBody!.Data!.Id;

        // Act — delete the non-primary package
        var deleteResponse = await Client.DeleteAsync($"/api/v1/packages/{secondPackageId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);  // Should have returned 200 OK

        // Verify the package is gone
        var getResponse = await Client.GetAsync($"/api/v1/packages/{secondPackageId}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode); // Should no longer find the deleted package
    }

    [Fact]
    public async Task Should_ReturnNotFound_WhenPackageDoesNotExist()
    {
        // Arrange
        var unknownPackageId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"/api/v1/packages/{unknownPackageId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);  // Should have returned 404 Not Found
    }
}
