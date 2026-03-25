using Modules.Sales.Domain.Packages.ProjectCosts;
using Modules.Sales.IntegrationTests.Abstractions;

namespace Modules.Sales.IntegrationTests.Packages;

public class UpdatePackageHomeTests(SalesTestFactory factory) : SalesIntegrationTestBase(factory)
{
    private string Endpoint => $"/api/v1/packages/{PackageId}/home";

    [Fact]
    public async Task Home_WithWaRental()
    {
        // Arrange + Act — ArrangeSaleWithHomeAsync performs the PUT to /home
        await ArrangeSaleWithHomeAsync(
            homeSalePrice: 80_000m,
            homeEstimatedCost: 60_000m,
            wheelAndAxles: Domain.Packages.Home.WheelAndAxlesOption.Rent);

        // Assert
        var updatedPackage = await GetPackageAsync();

        var waPc = Assert.Single(updatedPackage.ProjectCosts,
            projectCost => projectCost.CategoryNumber == ProjectCostCategories.WheelsAndAxles);
        Assert.Equal(ProjectCostItems.WaRental, waPc.ItemId); // Should use WaRental item for rental
        Assert.Equal(FakeiSeriesAdapter.WaSalePrice, waPc.SalePrice); // Should set SP from iSeries
        Assert.Equal(FakeiSeriesAdapter.WaCost, waPc.EstimatedCost); // Should set EC from iSeries

        // GP = (HomeSP - HomeEC) + (WaSP - WaEC) = (80000 - 60000) + (500 - 300) = 20200
        Assert.Equal(20_200m, updatedPackage.GrossProfit); // Should calculate gross profit correctly
        Assert.True(updatedPackage.MustRecalculateTaxes); // Should flag taxes for recalculation
    }

    [Fact]
    public async Task Home_WithWaPurchase()
    {
        // Arrange + Act — ArrangeSaleWithHomeAsync performs the PUT to /home
        await ArrangeSaleWithHomeAsync(
            homeSalePrice: 80_000m,
            homeEstimatedCost: 60_000m,
            wheelAndAxles: Domain.Packages.Home.WheelAndAxlesOption.Purchase);

        // Assert
        var updatedPackage = await GetPackageAsync();

        var waPc = Assert.Single(updatedPackage.ProjectCosts,
            projectCost => projectCost.CategoryNumber == ProjectCostCategories.WheelsAndAxles);
        Assert.Equal(ProjectCostItems.WaPurchase, waPc.ItemId); // Should use WaPurchase item for purchase
        Assert.Equal(FakeiSeriesAdapter.WaSalePrice, waPc.SalePrice); // Should set SP from iSeries
        Assert.Equal(FakeiSeriesAdapter.WaCost, waPc.EstimatedCost); // Should set EC from iSeries

        // GP = (HomeSP - HomeEC) + (WaSP - WaEC) = (80000 - 60000) + (500 - 300) = 20200
        Assert.Equal(20_200m, updatedPackage.GrossProfit); // Should calculate gross profit correctly
        Assert.True(updatedPackage.MustRecalculateTaxes); // Should flag taxes for recalculation
    }

    [Fact]
    public async Task Home_MultiSection_NoWheelAndAxlesProjectCostCreated()
    {
        // Arrange + Act — multi-section homes do not get W&A project costs
        await ArrangeSaleWithHomeAsync(
            homeSalePrice: 80_000m,
            homeEstimatedCost: 60_000m,
            numberOfFloorSections: 2,
            wheelAndAxles: Domain.Packages.Home.WheelAndAxlesOption.Rent);

        // Assert
        var updatedPackage = await GetPackageAsync();

        Assert.DoesNotContain(updatedPackage.ProjectCosts,
            projectCost => projectCost.CategoryNumber == ProjectCostCategories.WheelsAndAxles); // Should not create W&A PC for multi-section

        // GP = HomeSP - HomeEC = 80000 - 60000 = 20000 (no W&A contribution)
        Assert.Equal(20_000m, updatedPackage.GrossProfit); // Should calculate gross profit without W&A
        Assert.True(updatedPackage.MustRecalculateTaxes); // Should flag taxes for recalculation
    }
}
