using System.Net;
using System.Net.Http.Json;
using Modules.Sales.Domain.Packages.Land;
using Modules.Sales.Domain.Packages.ProjectCosts;
using Modules.Sales.EndpointTests.Abstractions;
using Modules.Sales.Presentation.Endpoints.V1.Packages.Land;

namespace Modules.Sales.EndpointTests.Packages;

public class UpdatePackageLandTests(SalesEndpointTestFixture fixture) : SalesEndpointTestBase(fixture)
{
    private string Endpoint => $"/api/v1/packages/{PackageId}/land";

    [Fact]
    public async Task Land_CustomerLandPayoff()
    {
        // Arrange
        var payoffAmount = 20_000m;
        await ArrangeSaleWithHomeAsync();
        var packageBeforeUpdate = await GetPackageAsync();

        // Act
        var response = await Client.PutAsJsonAsync(Endpoint, new UpdatePackageLandRequest(
            SalePrice: 50_000m,
            EstimatedCost: 50_000m,
            RetailSalePrice: 50_000m,
            LandPurchaseType: nameof(LandPurchaseType.CustomerHasLand),
            TypeOfLandWanted: null,
            CustomerLandType: nameof(CustomerLandType.CustomerOwnedLand),
            LandInclusion: nameof(LandInclusion.CustomerLandPayoff),
            LandStockNumber: null,
            LandSalesPrice: null,
            LandCost: null,
            PropertyOwner: "John Doe",
            FinancedBy: "Local Bank",
            EstimatedValue: 55_000m,
            SizeInAcres: 2.5m,
            PayoffAmountFinancing: payoffAmount,
            LandEquity: 35_000m,
            OriginalPurchaseDate: new DateTime(2018, 6, 15, 0, 0, 0, DateTimeKind.Utc),
            OriginalPurchasePrice: 30_000m,
            Realtor: null,
            PurchasePrice: null,
            PropertyOwnerPhoneNumber: null,
            PropertyLotRent: null,
            CommunityNumber: null,
            CommunityName: null,
            CommunityManagerName: null,
            CommunityManagerPhoneNumber: null,
            CommunityManagerEmail: null,
            CommunityMonthlyCost: null));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);                      // Should have returned 200 OK
        var updatedPackage = await GetPackageAsync();

        Assert.NotNull(updatedPackage.Land);                                       // Should create land section
        Assert.Equal(payoffAmount, updatedPackage.Land.SalePrice);                 // Should set land sale price to payoff amount
        Assert.Equal(payoffAmount, updatedPackage.Land.EstimatedCost);             // Should set land estimated cost to payoff amount
        Assert.True(updatedPackage.Land.ShouldExcludeFromPricing);                 // Should have been excluded from pricing (land is always excluded)

        var landPayoffPc = Assert.Single(updatedPackage.ProjectCosts,
            projectCost => projectCost.CategoryNumber == ProjectCostCategories.LandPayoff
               && projectCost.ItemId == ProjectCostItems.LandPayoff);
        Assert.Equal(payoffAmount, landPayoffPc.SalePrice);                        // Should mirror land sale price on land payoff project cost
        Assert.Equal(payoffAmount, landPayoffPc.EstimatedCost);                    // Should mirror land estimated cost on land payoff project cost
        Assert.True(landPayoffPc.ShouldExcludeFromPricing);                        // Should have excluded land payoff from pricing

        Assert.True(updatedPackage.MustRecalculateTaxes);                          // Should have flagged tax recalculation

        // GP unchanged — land SP == EC, so net contribution is zero
        Assert.Equal(packageBeforeUpdate.GrossProfit, updatedPackage.GrossProfit); // Should not change gross profit
    }

    [Fact]
    public async Task Land_LandPurchase()
    {
        // Arrange
        var purchasePrice = 75_000m;
        await ArrangeSaleWithHomeAsync();
        var packageBeforeUpdate = await GetPackageAsync();

        // Act
        var response = await Client.PutAsJsonAsync(Endpoint, new UpdatePackageLandRequest(
            SalePrice: purchasePrice,
            EstimatedCost: purchasePrice,
            RetailSalePrice: purchasePrice,
            LandPurchaseType: nameof(LandPurchaseType.CustomerWantsToPurchaseLand),
            TypeOfLandWanted: nameof(TypeOfLandWanted.LandPurchase),
            CustomerLandType: null,
            LandInclusion: null,
            LandStockNumber: null,
            LandSalesPrice: null,
            LandCost: null,
            PropertyOwner: null,
            FinancedBy: null,
            EstimatedValue: null,
            SizeInAcres: 5.0m,
            PayoffAmountFinancing: null,
            LandEquity: null,
            OriginalPurchaseDate: null,
            OriginalPurchasePrice: null,
            Realtor: "Jane Smith Realty",
            PurchasePrice: purchasePrice,
            PropertyOwnerPhoneNumber: null,
            PropertyLotRent: null,
            CommunityNumber: null,
            CommunityName: null,
            CommunityManagerName: null,
            CommunityManagerPhoneNumber: null,
            CommunityManagerEmail: null,
            CommunityMonthlyCost: null));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);                      // Should have returned 200 OK
        var updatedPackage = await GetPackageAsync();

        Assert.NotNull(updatedPackage.Land);                                       // Should have created land section
        Assert.Equal(purchasePrice, updatedPackage.Land.SalePrice);                // Should have set land sale price to purchase price
        Assert.Equal(purchasePrice, updatedPackage.Land.EstimatedCost);            // Should have set land estimated cost to purchase price
        Assert.True(updatedPackage.Land.ShouldExcludeFromPricing);                 // Should have been excluded from pricing (land is always excluded)

        var landPayoffPc = Assert.Single(updatedPackage.ProjectCosts,
            projectCost => projectCost.CategoryNumber == ProjectCostCategories.LandPayoff
               && projectCost.ItemId == ProjectCostItems.LandPayoff);
        Assert.Equal(purchasePrice, landPayoffPc.SalePrice);                       // Should have mirrored land sale price
        Assert.Equal(purchasePrice, landPayoffPc.EstimatedCost);                   // Should have mirrored land estimated cost
        Assert.True(landPayoffPc.ShouldExcludeFromPricing);                        // Should have excluded land payoff from pricing

        Assert.True(updatedPackage.MustRecalculateTaxes);                          // Should flag taxes for recalculation

        // GP unchanged — land SP == EC, so net contribution is zero
        Assert.Equal(packageBeforeUpdate.GrossProfit, updatedPackage.GrossProfit); // Should not change gross profit
    }

    [Fact]
    public async Task Land_HomeCenterOwned()
    {
        // Arrange
        var landSalesPrice = 90_000m;
        var landCost = 70_000m;
        await ArrangeSaleWithHomeAsync();
        var packageBeforeUpdate = await GetPackageAsync();

        // Seed a LandParcelCache entry for HomeCenterOwnedLand lookup
        await Fixture.SeedLandParcelCacheAsync("LOT-001", landCost);

        // Act
        var response = await Client.PutAsJsonAsync(Endpoint, new UpdatePackageLandRequest(
            SalePrice: landSalesPrice,
            EstimatedCost: landCost,
            RetailSalePrice: landSalesPrice,
            LandPurchaseType: nameof(LandPurchaseType.CustomerWantsToPurchaseLand),
            TypeOfLandWanted: nameof(TypeOfLandWanted.HomeCenterOwnedLand),
            CustomerLandType: null,
            LandInclusion: null,
            LandStockNumber: "LOT-001",
            LandSalesPrice: landSalesPrice,
            LandCost: landCost,
            PropertyOwner: null,
            FinancedBy: null,
            EstimatedValue: null,
            SizeInAcres: 3.0m,
            PayoffAmountFinancing: null,
            LandEquity: null,
            OriginalPurchaseDate: null,
            OriginalPurchasePrice: null,
            Realtor: null,
            PurchasePrice: null,
            PropertyOwnerPhoneNumber: null,
            PropertyLotRent: null,
            CommunityNumber: null,
            CommunityName: null,
            CommunityManagerName: null,
            CommunityManagerPhoneNumber: null,
            CommunityManagerEmail: null,
            CommunityMonthlyCost: null));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);                      // Should have returned 200 OK
        var updatedPackage = await GetPackageAsync();

        Assert.NotNull(updatedPackage.Land);                                       // Should create land section
        Assert.Equal(landSalesPrice, updatedPackage.Land.SalePrice);               // Should set land sale price
        Assert.Equal(landCost, updatedPackage.Land.EstimatedCost);                 // Should set land estimated cost (dealer margin)

        Assert.True(updatedPackage.MustRecalculateTaxes);                          // Should flag taxes for recalculation

        // GP unchanged — land contributes SP - EC but is offset by matching LandPayoff PC
        Assert.Equal(packageBeforeUpdate.GrossProfit, updatedPackage.GrossProfit); // Should not change gross profit
    }

    [Fact]
    public async Task Land_NoPricedType()
    {
        // Arrange
        await ArrangeSaleWithHomeAsync();
        var packageBeforeUpdate = await GetPackageAsync();

        // Act
        var response = await Client.PutAsJsonAsync(Endpoint, new UpdatePackageLandRequest(
            SalePrice: 0m,
            EstimatedCost: 0m,
            RetailSalePrice: 0m,
            LandPurchaseType: nameof(LandPurchaseType.CustomerHasLand),
            TypeOfLandWanted: null,
            CustomerLandType: nameof(CustomerLandType.PrivateProperty),
            LandInclusion: null,
            LandStockNumber: null,
            LandSalesPrice: null,
            LandCost: null,
            PropertyOwner: "Jane Owner",
            FinancedBy: null,
            EstimatedValue: null,
            SizeInAcres: 1.0m,
            PayoffAmountFinancing: null,
            LandEquity: null,
            OriginalPurchaseDate: null,
            OriginalPurchasePrice: null,
            Realtor: null,
            PurchasePrice: null,
            PropertyOwnerPhoneNumber: "6155551234",
            PropertyLotRent: 400m,
            CommunityNumber: null,
            CommunityName: null,
            CommunityManagerName: null,
            CommunityManagerPhoneNumber: null,
            CommunityManagerEmail: null,
            CommunityMonthlyCost: null));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);                      // Should have returned 200 OK
        var updatedPackage = await GetPackageAsync();

        Assert.NotNull(updatedPackage.Land);                                       // Should create land section
        Assert.Equal(0m, updatedPackage.Land.SalePrice);                           // Should set land sale price to zero
        Assert.Equal(0m, updatedPackage.Land.EstimatedCost);                       // Should set land estimated cost to zero
        Assert.True(updatedPackage.Land.ShouldExcludeFromPricing);                 // Should exclude PrivateProperty from pricing

        Assert.DoesNotContain(updatedPackage.ProjectCosts,
            projectCost => projectCost.CategoryNumber == ProjectCostCategories.LandPayoff
               && projectCost.ItemId == ProjectCostItems.LandPayoff);              // Should not create LandPayoff PC when no priced type

        // GP unchanged — no land pricing contribution
        Assert.Equal(packageBeforeUpdate.GrossProfit, updatedPackage.GrossProfit); // Should not change gross profit
    }
}
