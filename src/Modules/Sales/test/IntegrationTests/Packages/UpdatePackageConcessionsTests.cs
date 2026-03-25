using System.Net;
using System.Net.Http.Json;
using Modules.Sales.Domain.Packages.ProjectCosts;
using Modules.Sales.IntegrationTests.Abstractions;
using Modules.Sales.Presentation.Endpoints.V1.Packages.Concessions;

namespace Modules.Sales.IntegrationTests.Packages;

public class UpdatePackageConcessionsTests(SalesIntegrationTestFixture fixture) : SalesIntegrationTestBase(fixture)
{
    private string Endpoint => $"/api/v1/packages/{PackageId}/concessions";

    [Fact]
    public async Task Concession_WithAmount()
    {
        // Arrange
        var concessionAmount = 3_000m;
        await ArrangeSaleWithHomeAsync();
        var packageBeforeUpdate = await GetPackageAsync();

        // Act
        var response = await Client.PutAsJsonAsync(Endpoint, new UpdatePackageConcessionsRequest(Amount: concessionAmount));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);                                    // Should have returned 200 OK
        var updatedPackage = await GetPackageAsync();

        Assert.NotNull(updatedPackage.Concessions);                                              // Should create concessions credit line
        Assert.True(updatedPackage.Concessions.ShouldExcludeFromPricing);                        // Should exclude concessions from pricing

        var sellerPaidPc = Assert.Single(updatedPackage.ProjectCosts,
            projectCost => projectCost.CategoryNumber == ProjectCostCategories.SellerPaidClosingCost
               && projectCost.ItemId == ProjectCostItems.SellerPaidClosingCost);

        Assert.Equal(concessionAmount, sellerPaidPc.EstimatedCost);                              // Should set EC to concession amount

        // GP = packageBeforeUpdate.GP - concession = 20200 - 3000 = 17200
        Assert.Equal(packageBeforeUpdate.GrossProfit - concessionAmount, updatedPackage.GrossProfit); // Should reduce GP by concession amount
        Assert.True(updatedPackage.MustRecalculateTaxes);                                        // Should flag taxes for recalculation
    }

    [Fact]
    public async Task Concession_ZeroAmount()
    {
        // Arrange
        await ArrangeSaleWithHomeAsync();
        var packageBeforeUpdate = await GetPackageAsync();

        // Act
        var response = await Client.PutAsJsonAsync(Endpoint, new UpdatePackageConcessionsRequest(Amount: 0m));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);                                   // Should have returned 200 OK
        var updatedPackage = await GetPackageAsync();

        Assert.Null(updatedPackage.Concessions);                                                // Should not create concessions line for zero amount

        Assert.DoesNotContain(updatedPackage.ProjectCosts,
            projectCost => projectCost.CategoryNumber == ProjectCostCategories.SellerPaidClosingCost
               && projectCost.ItemId == ProjectCostItems.SellerPaidClosingCost);                // Should not create seller paid PC

        Assert.Equal(packageBeforeUpdate.GrossProfit, updatedPackage.GrossProfit);              // Should not change gross profit
    }

    [Fact]
    public async Task Concession_AmountChanged()
    {
        // Arrange
        var firstConcessionAmount = 3_000m;
        var updatedConcessionAmount = 5_000m;
        await ArrangeSaleWithHomeAsync();
        var packageBeforeUpdate = await GetPackageAsync();

        // Act — first concession at 3000, then update to 5000
        var response1 = await Client.PutAsJsonAsync(Endpoint, new UpdatePackageConcessionsRequest(Amount: firstConcessionAmount));
        var response2 = await Client.PutAsJsonAsync(Endpoint, new UpdatePackageConcessionsRequest(Amount: updatedConcessionAmount));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);                                               // Should have returned 200 OK
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);                                               // Should have returned 200 OK
        var updatedPackage = await GetPackageAsync();

        var sellerPaidPc = Assert.Single(updatedPackage.ProjectCosts,
            projectCost => projectCost.CategoryNumber == ProjectCostCategories.SellerPaidClosingCost
               && projectCost.ItemId == ProjectCostItems.SellerPaidClosingCost);
        Assert.Equal(updatedConcessionAmount, sellerPaidPc.EstimatedCost);                                   // Should update PC to new concession amount

        // GP = packageBeforeUpdate.GP - concession = 20200 - 5000 = 15200
        Assert.Equal(packageBeforeUpdate.GrossProfit - updatedConcessionAmount, updatedPackage.GrossProfit); // Should reduce GP by updated concession amount
    }
}
