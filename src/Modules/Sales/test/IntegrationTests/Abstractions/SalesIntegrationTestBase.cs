using System.Net.Http.Json;
using Bogus;
using Modules.Sales.Application.Packages.GetPackageById;
using Modules.Sales.Application.Packages.UpdatePackageHome;
using Modules.Sales.Domain.Packages.Home;
using Modules.Sales.Presentation.Endpoints.V1.DeliveryAddress;
using Modules.Sales.Presentation.Endpoints.V1.Packages;
using Modules.Sales.Presentation.Endpoints.V1.Sales;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.IntegrationTests.Abstractions;

// Base class for Sales module integration tests.
//
// Handles:
// - Test lifecycle (Respawn reset, reference data seeding)
// - Setup helpers that build up state step by step (ArrangeSaleAsync -> ArrangeSaleWithHomeAsync)
// - State tracking (SaleId, PackageId, etc.)
//
// Generic HTTP helpers (PutAndAssertOkAsync, GetAsync<T>, etc.) are extension methods
// on HttpClient in SalesHttpHelpers -- usable by any test, with or without this base class.
[Collection("SalesIntegration")]
public abstract class SalesIntegrationTestBase(SalesIntegrationTestFixture fixture) : IAsyncLifetime
{
    protected static readonly Faker Faker = new();

    protected readonly SalesIntegrationTestFixture Fixture = fixture;
    protected readonly HttpClient Client = fixture.CreateClient();

    protected Guid TestCustomerId => Fixture.TestCustomerId;
    protected int TestHomeCenterNumber => SalesIntegrationTestFixture.TestHomeCenterNumber;

    // Populated by setup helpers
    protected Guid SaleId { get; private set; }
    protected int SaleNumber { get; private set; }
    protected Guid PackageId { get; private set; }
    protected Guid DeliveryAddressId { get; private set; }

    // -- Lifecycle -------------------------------------------------------

    public async Task InitializeAsync()
    {
        await Fixture.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        Client.Dispose();
        return Task.CompletedTask;
    }

    // -- Setup helpers ---------------------------------------------------

    // Creates a sale. Sets SaleId and SaleNumber.
    protected async Task ArrangeSaleAsync()
    {
        var body = await Client.PostAsync<ApiEnvelope<CreateSaleResponse>>(
            "/api/v1/sales",
            new CreateSaleRequest(TestCustomerId, TestHomeCenterNumber));

        SaleId = body!.Data!.Id;
        SaleNumber = body.Data.SaleNumber;
    }

    // Creates a sale + delivery address. Sets SaleId, SaleNumber, DeliveryAddressId.
    protected async Task ArrangeSaleWithDeliveryAsync()
    {
        await ArrangeSaleAsync();

        var body = await Client.PostAsync<ApiEnvelope<CreateDeliveryAddressResponse>>(
            $"/api/v1/sales/{SaleId}/delivery-address",
            new CreateDeliveryAddressRequest(
                OccupancyType: "Primary Residence",
                IsWithinCityLimits: true,
                AddressLine1: "123 Test St",
                City: "Nashville",
                County: "Davidson",
                State: "TN",
                PostalCode: "37201"));

        DeliveryAddressId = body!.Data!.Id;
    }

    // Creates a sale + delivery address + package. Sets SaleId, SaleNumber, DeliveryAddressId, PackageId.
    protected async Task ArrangeSaleWithPackageAsync(string packageName = "Primary")
    {
        await ArrangeSaleWithDeliveryAsync();

        var body = await Client.PostAsync<ApiEnvelope<CreatePackageResponse>>(
            $"/api/v1/sales/{SaleId}/packages",
            new CreatePackageRequest(packageName));

        PackageId = body!.Data!.Id;
    }

    // Creates sale + delivery + package + baseline home. Full setup for most package tests.
    protected async Task ArrangeSaleWithHomeAsync(
        decimal homeSalePrice = 80_000m,
        decimal homeEstimatedCost = 60_000m,
        decimal homeRetailSalePrice = 80_000m,
        int numberOfFloorSections = 1,
        WheelAndAxlesOption wheelAndAxles = WheelAndAxlesOption.Rent)
    {
        await ArrangeSaleWithPackageAsync();

        await Client.PutAndAssertOkAsync(
            $"/api/v1/packages/{PackageId}/home",
            new UpdatePackageHomeRequest(
                SalePrice: homeSalePrice,
                EstimatedCost: homeEstimatedCost,
                RetailSalePrice: homeRetailSalePrice,
                StockNumber: null,
                HomeType: HomeType.New,
                HomeSourceType: HomeSourceType.Manual,
                ModularType: ModularType.Hud,
                Vendor: "TestVendor",
                Make: "TestMake",
                Model: "TestModel",
                ModelYear: 2026,
                LengthInFeet: 76.0m,
                WidthInFeet: 28.0m,
                Bedrooms: 3,
                Bathrooms: 2.0m,
                SquareFootage: "2128",
                SerialNumbers: ["TEST123AB"],
                BaseCost: homeEstimatedCost * 0.8m,
                OptionsCost: homeEstimatedCost * 0.12m,
                FreightCost: homeEstimatedCost * 0.08m,
                InvoiceCost: homeEstimatedCost,
                NetInvoice: homeEstimatedCost * 0.98m,
                GrossCost: homeEstimatedCost,
                TaxIncludedOnInvoice: 0m,
                NumberOfWheels: 4,
                NumberOfAxles: 2,
                WheelAndAxlesOption: wheelAndAxles,
                NumberOfFloorSections: numberOfFloorSections,
                CarrierFrameDeposit: 500m,
                RebateOnMfgInvoice: 0m,
                ClaytonBuilt: true,
                BuildType: numberOfFloorSections > 1 ? "Double" : "Single",
                InventoryReferenceId: null,
                StateAssociationAndMhiDues: 150m,
                PartnerAssistance: 0m,
                DistanceMiles: 50.0,
                PropertyType: null,
                PurchaseOption: null,
                ListingPrice: null,
                AccountNumber: null,
                DisplayAccountId: null,
                StreetAddress: null,
                City: null,
                State: null,
                ZipCode: null));
    }

    // GET the current package detail using the stored PackageId.
    protected async Task<PackageDetailResponse> GetPackageAsync()
        => await Client.GetPackageAsync(PackageId);
}
