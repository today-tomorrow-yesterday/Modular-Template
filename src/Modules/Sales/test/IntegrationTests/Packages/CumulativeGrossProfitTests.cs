using System.Net;
using System.Net.Http.Json;
using Modules.Sales.Domain.Packages.Home;
using Modules.Sales.Domain.Packages.Land;
using Modules.Sales.IntegrationTests.Abstractions;
using Modules.Sales.Presentation.Endpoints.V1.Packages.Concessions;
using Modules.Sales.Presentation.Endpoints.V1.Packages.DownPayment;
using Modules.Sales.Presentation.Endpoints.V1.Packages.Land;
using Modules.Sales.Presentation.Endpoints.V1.Packages.Warranty;

namespace Modules.Sales.IntegrationTests.Packages;

public class CumulativeGrossProfitTests(SalesIntegrationTestFixture fixture) : SalesIntegrationTestBase(fixture)
{
    [Fact]
    public async Task FullJourney_GrossProfitAccumulatesCorrectly()
    {
        // ── Step 1: Home + W&A Rental ──────────────────────────────────
        // Arrange + Act
        await ArrangeSaleWithHomeAsync(
            homeSalePrice: 80_000m,
            homeEstimatedCost: 60_000m,
            wheelAndAxles: WheelAndAxlesOption.Rent);

        // Assert
        var package = await GetPackageAsync();
        // GP = (HomeSP - HomeEC) + (WaSP - WaEC) = (80000 - 60000) + (500 - 300) = 20200
        Assert.Equal(20_200m, package.GrossProfit); // Should start with home + W&A gross profit

        // ── Step 2: Land (PrivateProperty, excluded from pricing) ──────
        // Act
        var landResponse = await Client.PutAsJsonAsync(
            $"/api/v1/packages/{PackageId}/land",
            new UpdatePackageLandRequest(
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
        Assert.Equal(HttpStatusCode.OK, landResponse.StatusCode); // Should have returned 200 OK
        package = await GetPackageAsync();
        // GP unchanged — PrivateProperty is excluded, SP=EC=0
        Assert.Equal(20_200m, package.GrossProfit); // Should not change GP for excluded land

        // ── Step 3: Warranty (selected, $1200) ─────────────────────────
        // Act
        var warrantyResponse = await Client.PutAsJsonAsync(
            $"/api/v1/packages/{PackageId}/warranty",
            new UpdatePackageWarrantyRequest(WarrantySelected: true, WarrantyAmount: 1_200m));

        // Assert
        Assert.Equal(HttpStatusCode.OK, warrantyResponse.StatusCode); // Should have returned 200 OK
        package = await GetPackageAsync();
        // GP = 20200 + warrantySP(1200) = 21400
        Assert.Equal(21_400m, package.GrossProfit); // Should increase GP by warranty sale price

        // ── Step 4: Concession ($3000) ─────────────────────────────────
        // Act
        var concessionsResponse = await Client.PutAsJsonAsync(
            $"/api/v1/packages/{PackageId}/concessions",
            new UpdatePackageConcessionsRequest(Amount: 3_000m));

        // Assert
        Assert.Equal(HttpStatusCode.OK, concessionsResponse.StatusCode); // Should have returned 200 OK
        package = await GetPackageAsync();
        // GP = 21400 - concessionEC(3000) = 18400
        Assert.Equal(18_400m, package.GrossProfit); // Should reduce GP by concession amount

        // ── Step 5: Down Payment ($5000, excluded from pricing) ────────
        // Act
        var downPaymentResponse = await Client.PutAsJsonAsync(
            $"/api/v1/packages/{PackageId}/down-payment",
            new UpdatePackageDownPaymentRequest(Amount: 5_000m));

        // Assert
        Assert.Equal(HttpStatusCode.OK, downPaymentResponse.StatusCode); // Should have returned 200 OK
        package = await GetPackageAsync();
        // GP = 18400 — down payment is excluded from pricing, no GP impact
        Assert.Equal(18_400m, package.GrossProfit); // Should not change GP for excluded down payment
    }
}
