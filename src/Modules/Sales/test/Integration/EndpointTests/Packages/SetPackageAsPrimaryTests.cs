using System.Net;
using System.Net.Http.Json;
using Modules.Sales.Application.Packages.GetPackageById;
using Modules.Sales.EndpointTests.Abstractions;
using Modules.Sales.Presentation.Endpoints.V1.Packages;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.EndpointTests.Packages;

// PATCH /api/v1/packages/{packageId}?action=set-as-primary
//
// Tests:
// - Set second package as primary -> 200 OK, second is now primary, first is not
// - Unknown package ID -> 404 Not Found
public class SetPackageAsPrimaryTests(SalesEndpointTestFixture fixture) : SalesEndpointTestBase(fixture)
{
    [Fact]
    public async Task Should_SetAsPrimary_WhenPackageExists()
    {
        // Arrange — create sale with one package, then add a second
        await ArrangeSaleWithPackageAsync("First");
        var firstPackageId = PackageId;

        var secondBody = await Client.PostAsync<ApiEnvelope<CreatePackageResponse>>(
            $"/api/v1/sales/{SaleId}/packages",
            new CreatePackageRequest(SaleId, "Second"));
        var secondPackageId = secondBody!.Data!.Id;

        // Act — set the second package as primary
        var patchResponse = await Client.PatchAsync(
            $"/api/v1/packages/{secondPackageId}?action=set-as-primary", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode);          // Should have returned 200 OK

        // Verify second package is now primary
        var secondPackage = await Client.GetAsync<ApiEnvelope<PackageDetailResponse>>(
            $"/api/v1/packages/{secondPackageId}");
        Assert.True(secondPackage!.Data!.IsPrimaryPackage);                  // Second package should be primary

        // Verify first package is no longer primary
        var firstPackage = await Client.GetAsync<ApiEnvelope<PackageDetailResponse>>(
            $"/api/v1/packages/{firstPackageId}");
        Assert.False(firstPackage!.Data!.IsPrimaryPackage);                  // First package should no longer be primary
    }

    [Fact]
    public async Task Should_ReturnNotFound_WhenPackageDoesNotExist()
    {
        // Arrange
        var unknownPackageId = Guid.NewGuid();

        // Act
        var response = await Client.PatchAsync(
            $"/api/v1/packages/{unknownPackageId}?action=set-as-primary", null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);         // Should have returned 404 Not Found
    }
}
