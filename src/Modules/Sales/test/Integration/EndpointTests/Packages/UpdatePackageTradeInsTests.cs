using System.Net;
using System.Net.Http.Json;
using Modules.Sales.Domain.Packages.ProjectCosts;
using Modules.Sales.EndpointTests.Abstractions;
using Modules.Sales.Presentation.Endpoints.V1.Packages.TradeIns;

namespace Modules.Sales.EndpointTests.Packages;

public class UpdatePackageTradeInsTests(SalesEndpointTestFixture fixture) : SalesEndpointTestBase(fixture)
{
    private string Endpoint => $"/api/v1/packages/{PackageId}/trade-ins";

    private static UpdatePackageTradeInItem MakeTradeIn(
        decimal tradeAllowance,
        decimal payoffAmount,
        decimal bookInAmount) =>
        new(
            SalePrice: 0m,
            EstimatedCost: 0m,
            RetailSalePrice: 0m,
            TradeType: "Manufactured Home",
            Year: 2015,
            Make: "Clayton",
            Model: "Freedom",
            FloorWidth: 14.0m,
            FloorLength: 70.0m,
            TradeAllowance: tradeAllowance,
            PayoffAmount: payoffAmount,
            BookInAmount: bookInAmount);

    [Fact]
    public async Task TradeIn_OverAllowance()
    {
        // Arrange
        await ArrangeSaleWithHomeAsync();
        var packageBeforeUpdate = await GetPackageAsync();

        // TradeAllowance 20000 > BookInAmount 15000 → over-allowance = 5000
        var tradeIn = MakeTradeIn(tradeAllowance: 20_000m, payoffAmount: 5_000m, bookInAmount: 15_000m);

        // Act
        var response = await Client.PutAsJsonAsync(Endpoint, new[] { tradeIn });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);                               // Should have returned 200 OK
        var updatedPackage = await GetPackageAsync();

        var overAllowancePc = Assert.Single(updatedPackage.ProjectCosts,
            projectCost => projectCost.CategoryNumber == ProjectCostCategories.TradeOverAllowance
               && projectCost.ItemId == ProjectCostItems.TradeOverAllowance);
        Assert.Equal(tradeIn.TradeAllowance - tradeIn.BookInAmount, overAllowancePc.EstimatedCost); // Should set EC to over-allowance amount

        // GP = packageBeforeUpdate.GP - overAllowance = 20200 - 5000 = 15200
        Assert.Equal(packageBeforeUpdate.GrossProfit - (tradeIn.TradeAllowance - tradeIn.BookInAmount), updatedPackage.GrossProfit); // Should reduce GP by over-allowance
    }

    [Fact]
    public async Task TradeIn_AtBookValue()
    {
        // Arrange
        await ArrangeSaleWithHomeAsync();
        var packageBeforeUpdate = await GetPackageAsync();

        // TradeAllowance == BookInAmount → no over-allowance
        var tradeIn = MakeTradeIn(tradeAllowance: 15_000m, payoffAmount: 5_000m, bookInAmount: 15_000m);

        // Act
        var response = await Client.PutAsJsonAsync(Endpoint, new[] { tradeIn });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);                      // Should have returned 200 OK
        var updatedPackage = await GetPackageAsync();

        Assert.DoesNotContain(updatedPackage.ProjectCosts,
            projectCost => projectCost.CategoryNumber == ProjectCostCategories.TradeOverAllowance
               && projectCost.ItemId == ProjectCostItems.TradeOverAllowance);      // Should not create over-allowance PC

        Assert.Equal(packageBeforeUpdate.GrossProfit, updatedPackage.GrossProfit); // Should not change gross profit
    }

    [Fact]
    public async Task TradeIns_Multiple_OnlyOverAllowanceGetsProjectCost()
    {
        // Arrange
        await ArrangeSaleWithHomeAsync();
        var packageBeforeUpdate = await GetPackageAsync();

        // Trade 1: over-allowance by 3000; Trade 2: at book value
        var overTrade = MakeTradeIn(tradeAllowance: 18_000m, payoffAmount: 3_000m, bookInAmount: 15_000m);
        var evenTrade = MakeTradeIn(tradeAllowance: 10_000m, payoffAmount: 2_000m, bookInAmount: 10_000m);

        // Act
        var response = await Client.PutAsJsonAsync(Endpoint, new[] { overTrade, evenTrade });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);    // Should have returned 200 OK
        var updatedPackage = await GetPackageAsync();

        var overAllowancePcs = updatedPackage.ProjectCosts
            .Where(projectCost => projectCost.CategoryNumber == ProjectCostCategories.TradeOverAllowance
                      && projectCost.ItemId == ProjectCostItems.TradeOverAllowance)
            .ToArray();
        Assert.Single(overAllowancePcs);                         // Should create only one over-allowance PC for the trade that exceeded book value
        Assert.Equal(overTrade.TradeAllowance - overTrade.BookInAmount, overAllowancePcs[0].EstimatedCost); // Should set EC to over-allowance amount for that trade
    }
}
