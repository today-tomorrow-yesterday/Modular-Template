using System.Net;
using System.Net.Http.Json;
using Modules.Sales.EndpointTests.Abstractions;
using Modules.Sales.Presentation.Endpoints.V1.Packages.Warranty;

namespace Modules.Sales.EndpointTests.Packages;

public class UpdatePackageWarrantyTests(SalesEndpointTestFixture fixture) : SalesEndpointTestBase(fixture)
{
    private string Endpoint => $"/api/v1/packages/{PackageId}/warranty";

    [Fact]
    public async Task Warranty_Selected()
    {
        // Arrange
        var warrantyAmount = 1_200m;
        await ArrangeSaleWithHomeAsync();
        var packageBeforeUpdate = await GetPackageAsync();

        // Act
        var response = await Client.PutAsJsonAsync(Endpoint, new UpdatePackageWarrantyRequest(
            WarrantySelected: true,
            WarrantyAmount: warrantyAmount,
            SalesTaxPremium: 0m,
            SalePrice: warrantyAmount,
            EstimatedCost: 0m,
            RetailSalePrice: warrantyAmount,
            ShouldExcludeFromPricing: false));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);                               // Should have returned 200 OK
        var updatedPackage = await GetPackageAsync();

        Assert.NotNull(updatedPackage.Warranty);                                            // Should create warranty section
        Assert.Equal(warrantyAmount, updatedPackage.Warranty.SalePrice);                    // Should set SP to warranty amount
        Assert.Equal(0m, updatedPackage.Warranty.EstimatedCost);                            // Should set EC to zero (pure revenue)

        // GP = packageBeforeUpdate.GP + warrantySP = before + 1200
        Assert.Equal(packageBeforeUpdate.GrossProfit + warrantyAmount, updatedPackage.GrossProfit); // Should increase GP by warranty sale price
        Assert.True(updatedPackage.MustRecalculateTaxes);                                   // Should flag taxes for recalculation
    }

    [Fact]
    public async Task Warranty_NotSelected()
    {
        // Arrange
        await ArrangeSaleWithHomeAsync();
        var packageBeforeUpdate = await GetPackageAsync();

        // Act
        var response = await Client.PutAsJsonAsync(Endpoint, new UpdatePackageWarrantyRequest(
            WarrantySelected: false,
            WarrantyAmount: 0m,
            SalesTaxPremium: 0m,
            SalePrice: 0m,
            EstimatedCost: 0m,
            RetailSalePrice: 0m,
            ShouldExcludeFromPricing: false));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);                      // Should have returned 200 OK
        var updatedPackage = await GetPackageAsync();

        Assert.NotNull(updatedPackage.Warranty);                                   // Should create warranty section even when not selected
        Assert.Equal(0m, updatedPackage.Warranty.SalePrice);                       // Should set SP to zero
        Assert.Equal(0m, updatedPackage.Warranty.EstimatedCost);                   // Should set EC to zero

        Assert.Equal(packageBeforeUpdate.GrossProfit, updatedPackage.GrossProfit); // Should not change gross profit
        Assert.True(updatedPackage.MustRecalculateTaxes);                          // Should flag taxes for recalculation
    }

    [Fact]
    public async Task Warranty_ResaveSameValues()
    {
        // Arrange
        var warrantyAmount = 1_200m;
        await ArrangeSaleWithHomeAsync();

        var request = new UpdatePackageWarrantyRequest(
            WarrantySelected: true,
            WarrantyAmount: warrantyAmount,
            SalesTaxPremium: 0m,
            SalePrice: warrantyAmount,
            EstimatedCost: 0m,
            RetailSalePrice: warrantyAmount,
            ShouldExcludeFromPricing: false);

        // Act — save same values twice
        var response1 = await Client.PutAsJsonAsync(Endpoint, request);
        var response2 = await Client.PutAsJsonAsync(Endpoint, request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);        // Should have returned 200 OK
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);        // Should have returned 200 OK
        var updatedPackage = await GetPackageAsync();

        Assert.True(updatedPackage.MustRecalculateTaxes);             // Should persist MustRecalculateTaxes from first save
        Assert.Equal(warrantyAmount, updatedPackage.Warranty!.SalePrice); // Should retain warranty sale price on resave
    }
}
