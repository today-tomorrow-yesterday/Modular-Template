using System.Net;
using System.Net.Http.Json;
using Modules.Sales.Application.Packages.GetPackageById;
using Modules.Sales.Application.Packages.UpdatePackageHome;
using Modules.Sales.Domain.Packages.Home;
using Modules.Sales.IntegrationTests.Abstractions;
using Modules.Sales.Presentation.Endpoints.V1.DeliveryAddress;
using Modules.Sales.Presentation.Endpoints.V1.Packages;
using Modules.Sales.Presentation.Endpoints.V1.Sales;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.IntegrationTests.Sales;

// End-to-end lifecycle tests simulating a real user session on the website.
// Each test creates fresh data — no dependency on pre-seeded records.
//
// Tests:
// - Full journey: Create Sale -> Delivery Address -> Package -> Update Home -> GET package back,
//   verify make/model persisted and gross profit calculated (multi-section, no W&A)
// - Duplicate delivery address -> 409 Conflict (only one per sale)
// - Duplicate package name -> 409 Conflict (names are unique within a sale)
public class SaleLifecycleTests(SalesTestFactory factory) : SalesIntegrationTestBase(factory)
{
    [Fact]
    public async Task FullJourney_CreateSale_DeliveryAddress_Package_UpdateHome()
    {
        // -- Step 1: Create a sale -------------------
        await ArrangeSaleAsync();
        Assert.NotEqual(Guid.Empty, SaleId);
        Assert.True(SaleNumber > 0);

        // -- Step 2: Create delivery address ---------
        var addressRequest = new CreateDeliveryAddressRequest(
            OccupancyType: "Primary Residence",
            IsWithinCityLimits: true,
            AddressLine1: "5000 Clayton Rd",
            City: "Maryville",
            County: "Blount",
            State: "TN",
            PostalCode: "37801");

        var addressResponse = await Client.PostAsJsonAsync(
            $"/api/v1/sales/{SaleId}/delivery-address", addressRequest);
        Assert.Equal(HttpStatusCode.Created, addressResponse.StatusCode);

        // -- Step 3: Create a package ----------------
        var packageResponse = await Client.PostAsync<ApiEnvelope<CreatePackageResponse>>(
            $"/api/v1/sales/{SaleId}/packages",
            new CreatePackageRequest("Primary"));
        var packageId = packageResponse!.Data!.Id;
        Assert.NotEqual(Guid.Empty, packageId);

        // -- Step 4: Update home section -------------
        var homeRequest = new UpdatePackageHomeRequest(
            SalePrice: 340_000m,
            EstimatedCost: 250_000m,
            RetailSalePrice: 350_000m,
            StockNumber: null,
            HomeType: HomeType.New,
            HomeSourceType: HomeSourceType.Manual,
            ModularType: ModularType.Hud,
            Vendor: "CMH Manufacturing",
            Make: "Clayton",
            Model: "Summit",
            ModelYear: 2025,
            LengthInFeet: 76.0m,
            WidthInFeet: 28.0m,
            Bedrooms: 3,
            Bathrooms: 2.0m,
            SquareFootage: "2128",
            SerialNumbers: ["CLT834205AB"],
            BaseCost: 200_000m,
            OptionsCost: 30_000m,
            FreightCost: 20_000m,
            InvoiceCost: 250_000m,
            NetInvoice: 245_000m,
            GrossCost: 250_000m,
            TaxIncludedOnInvoice: 5_000m,
            NumberOfWheels: 16,
            NumberOfAxles: 4,
            WheelAndAxlesOption: WheelAndAxlesOption.Rent,
            NumberOfFloorSections: 2,
            CarrierFrameDeposit: 500m,
            RebateOnMfgInvoice: 5_000m,
            ClaytonBuilt: true,
            BuildType: "Double",
            InventoryReferenceId: null,
            StateAssociationAndMhiDues: 150m,
            PartnerAssistance: 500m,
            DistanceMiles: 125.5,
            PropertyType: "DoubleWide",
            PurchaseOption: "Finance",
            ListingPrice: 355_000m,
            AccountNumber: "ACC-100234",
            DisplayAccountId: "DA-1001",
            StreetAddress: "5000 Clayton Rd",
            City: "Maryville",
            State: "TN",
            ZipCode: "37801");

        var homeResponse = await Client.PutAsJsonAsync(
            $"/api/v1/packages/{packageId}/home", homeRequest);
        Assert.Equal(HttpStatusCode.OK, homeResponse.StatusCode);

        var homeBody = await homeResponse.Content
            .ReadFromJsonAsync<ApiEnvelope<PackageUpdatedResponse>>();
        // Gross Profit = (Home Sale Price - Home Estimated Cost), no Wheels & Axles for multi-section
        // Gross Profit = (340000 - 250000) = 90000
        Assert.Equal(90_000m, homeBody!.Data!.GrossProfit);                        // Should have calculated correct gross profit

        // -- Step 5: GET package back -- verify persistence -
        var updatedPackage = await Client.GetAsync<ApiEnvelope<PackageDetailResponse>>(
            $"/api/v1/packages/{packageId}");
        Assert.NotNull(updatedPackage?.Data);                                              // Should have returned the package
        Assert.Equal("Primary", updatedPackage.Data.Name);                         // Should have saved the package name
        Assert.NotNull(updatedPackage.Data.Home);                                          // Should have saved the home section
        Assert.Equal("Clayton", updatedPackage.Data.Home.Make);                    // Should have saved the correct make
        Assert.Equal("Summit", updatedPackage.Data.Home.Model);                    // Should have saved the correct model
    }

    [Fact]
    public async Task Should_ReturnConflict_WhenDuplicateDeliveryAddress()
    {
        // Arrange
        await ArrangeSaleAsync();

        var addressRequest = new CreateDeliveryAddressRequest(
            "Primary Residence", true, "123 Main St", "Maryville", "Blount", "TN", "37801");

        // Act
        var first = await Client.PostAsJsonAsync($"/api/v1/sales/{SaleId}/delivery-address", addressRequest);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);                    // Should have created first address

        var second = await Client.PostAsJsonAsync($"/api/v1/sales/{SaleId}/delivery-address", addressRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);                  // Should have rejected duplicate address
    }

    [Fact]
    public async Task Should_ReturnConflict_WhenDuplicatePackageName()
    {
        // Arrange
        await ArrangeSaleAsync();

        // Act
        var first = await Client.PostAsJsonAsync(
            $"/api/v1/sales/{SaleId}/packages", new CreatePackageRequest("Primary"));
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);                    // Should have created first package

        var second = await Client.PostAsJsonAsync(
            $"/api/v1/sales/{SaleId}/packages", new CreatePackageRequest("Primary"));

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);                  // Should have rejected duplicate package name
    }
}
