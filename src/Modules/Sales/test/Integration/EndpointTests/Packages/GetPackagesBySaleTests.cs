using System.Net;
using System.Net.Http.Json;
using Modules.Sales.Application.Packages.GetPackagesBySale;
using Modules.Sales.EndpointTests.Abstractions;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.EndpointTests.Packages;

// GET /api/v1/sales/{saleId}/packages
//
// Tests:
// - Sale with packages -> 200 OK with package summaries
// - Sale with delivery but no packages -> 200 OK with empty list
public class GetPackagesBySaleTests(SalesEndpointTestFixture fixture) : SalesEndpointTestBase(fixture)
{
    [Fact]
    public async Task Should_ReturnPackages_WhenPackagesExist()
    {
        // Arrange
        await ArrangeSaleWithPackageAsync();

        // Act
        var body = await Client.GetAsync<ApiEnvelope<IReadOnlyCollection<PackageSummaryResponse>>>(
            $"/api/v1/sales/{SaleId}/packages");

        // Assert
        Assert.NotNull(body?.Data);                                  // Should have returned package data
        var package = Assert.Single(body.Data);                      // Should have exactly one package
        Assert.Equal(PackageId, package.Id);                         // Should match the created package ID
        Assert.Equal("Primary", package.Name);                       // Should match the package name
        Assert.True(package.IsPrimaryPackage);                       // First package should be primary
    }

    [Fact]
    public async Task Should_ReturnEmpty_WhenNoPackages()
    {
        // Arrange — sale with delivery but no packages
        await ArrangeSaleWithDeliveryAsync();

        // Act
        var body = await Client.GetAsync<ApiEnvelope<IReadOnlyCollection<PackageSummaryResponse>>>(
            $"/api/v1/sales/{SaleId}/packages");

        // Assert
        Assert.NotNull(body?.Data);                                  // Should have returned a response
        Assert.Empty(body.Data);                                     // Should have returned an empty list
    }
}
