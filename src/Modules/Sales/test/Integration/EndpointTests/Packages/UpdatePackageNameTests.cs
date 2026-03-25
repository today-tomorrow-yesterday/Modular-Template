using System.Net;
using System.Net.Http.Json;
using Modules.Sales.EndpointTests.Abstractions;
using Modules.Sales.Presentation.Endpoints.V1.Packages;

namespace Modules.Sales.EndpointTests.Packages;

// PATCH /api/v1/packages/{packageId}/name
//
// Tests:
// - Valid rename -> 200 OK, GET package back verifies new name
// - Unknown package ID -> 404 Not Found
public class UpdatePackageNameTests(SalesEndpointTestFixture fixture) : SalesEndpointTestBase(fixture)
{
    [Fact]
    public async Task Should_UpdateName_WhenRequestIsValid()
    {
        // Arrange
        await ArrangeSaleWithPackageAsync("Original");

        // Act
        var request = new UpdatePackageNameRequest("Renamed");
        var patchResponse = await Client.PatchAsJsonAsync(
            $"/api/v1/packages/{PackageId}/name", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode);   // Should have returned 200 OK

        // Verify the name changed
        var updatedPackage = await GetPackageAsync();
        Assert.Equal("Renamed", updatedPackage.Name);                 // Should have updated the package name
    }

    [Fact]
    public async Task Should_ReturnNotFound_WhenPackageDoesNotExist()
    {
        // Arrange
        var unknownPackageId = Guid.NewGuid();
        var request = new UpdatePackageNameRequest("Anything");

        // Act
        var response = await Client.PatchAsJsonAsync(
            $"/api/v1/packages/{unknownPackageId}/name", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);  // Should have returned 404 Not Found
    }
}
