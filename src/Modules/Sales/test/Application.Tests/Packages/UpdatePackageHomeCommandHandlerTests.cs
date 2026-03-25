using Modules.Sales.Application.Packages.UpdatePackageHome;
using Modules.Sales.Domain;
using Modules.Sales.Domain.InventoryCache;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Home;
using Modules.Sales.Domain.Packages.ProjectCosts;
using RetailLocationCacheEntity = Modules.Sales.Domain.RetailLocationCache.RetailLocationCache;
using Modules.Sales.Domain.Sales;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Rtl.Core.Application.Adapters.ISeries;
using Rtl.Core.Application.Adapters.ISeries.Pricing;
using Rtl.Core.Application.Persistence;
using System.Reflection;
using Xunit;

namespace Modules.Sales.Application.Tests.Packages;

public sealed class UpdatePackageHomeCommandHandlerTests
{
    private readonly IPackageRepository _packageRepository = Substitute.For<IPackageRepository>();
    private readonly IInventoryCacheQueries _inventoryCacheQueries = Substitute.For<IInventoryCacheQueries>();
    private readonly IiSeriesAdapter _iSeriesAdapter = Substitute.For<IiSeriesAdapter>();
    private readonly IUnitOfWork<ISalesModule> _unitOfWork = Substitute.For<IUnitOfWork<ISalesModule>>();
    private readonly ILogger<UpdatePackageHomeCommandHandler> _logger = Substitute.For<ILogger<UpdatePackageHomeCommandHandler>>();
    private readonly UpdatePackageHomeCommandHandler _sut;

    public UpdatePackageHomeCommandHandlerTests()
    {
        _sut = new UpdatePackageHomeCommandHandler(
            _packageRepository, _inventoryCacheQueries, _iSeriesAdapter, _unitOfWork, _logger);
    }

    [Fact]
    public async Task Returns_failure_when_package_not_found()
    {
        var publicId = Guid.NewGuid();
        _packageRepository.GetByPublicIdWithSaleContextAsync(publicId, Arg.Any<CancellationToken>())
            .Returns((Package?)null);

        var result = await _sut.Handle(
            new UpdatePackageHomeCommand(publicId, CreateHomeRequest()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Package.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Returns_failure_when_on_lot_home_not_found_in_inventory_cache()
    {
        var package = CreatePackageWithSaleContext();
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);
        _inventoryCacheQueries.FindByHomeCenterAndStockAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((OnLotHomeCache?)null);

        var home = CreateHomeRequest(homeSourceType: HomeSourceType.OnLot, stockNumber: "STK-999");
        var result = await _sut.Handle(
            new UpdatePackageHomeCommand(package.PublicId, home), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("OnLotHome.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Returns_success_on_happy_path()
    {
        var package = CreatePackageWithSaleContext();
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        // Quoted doesn't need inventory lookup
        var home = CreateHomeRequest(homeSourceType: HomeSourceType.Quoted);
        var result = await _sut.Handle(
            new UpdatePackageHomeCommand(package.PublicId, home), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Multi_section_home_does_not_add_wa_project_cost()
    {
        // Multi-section (NumberOfFloorSections > 1) homes ship on their own chassis;
        // legacy always stripped W&A lines regardless of WheelAndAxlesOption.
        var package = CreatePackageWithSaleContext();
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        _iSeriesAdapter.CalculateWheelAndAxlePriceByCount(Arg.Any<WheelAndAxlePriceByCountRequest>(), Arg.Any<CancellationToken>())
            .Returns(new WheelAndAxlePriceResult(SalePrice: 500m, Cost: 400m));

        var home = CreateHomeRequest(
            numberOfFloorSections: 2,
            wheelAndAxlesOption: WheelAndAxlesOption.Rent,
            numberOfWheels: 6,
            numberOfAxles: 3);

        var result = await _sut.Handle(
            new UpdatePackageHomeCommand(package.PublicId, home), CancellationToken.None);

        Assert.True(result.IsSuccess);

        // No W&A project cost lines should exist for multi-section homes
        var waLines = package.Lines.OfType<ProjectCostLine>()
            .Where(l => l.Details?.CategoryId == ProjectCostCategories.WheelsAndAxles);
        Assert.Empty(waLines);
    }

    [Fact]
    public async Task Single_section_home_adds_wa_project_cost_when_option_set()
    {
        // Single-section (NumberOfFloorSections <= 1) homes need W&A transport.
        // When WheelAndAxlesOption is set, a W&A project cost line should be added.
        var package = CreatePackageWithSaleContext();
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        _iSeriesAdapter.CalculateWheelAndAxlePriceByCount(Arg.Any<WheelAndAxlePriceByCountRequest>(), Arg.Any<CancellationToken>())
            .Returns(new WheelAndAxlePriceResult(SalePrice: 500m, Cost: 400m));

        var home = CreateHomeRequest(
            numberOfFloorSections: 1,
            wheelAndAxlesOption: WheelAndAxlesOption.Rent,
            numberOfWheels: 6,
            numberOfAxles: 3);

        var result = await _sut.Handle(
            new UpdatePackageHomeCommand(package.PublicId, home), CancellationToken.None);

        Assert.True(result.IsSuccess);

        // Single-section with Rent option → W&A Rental line should exist
        var waLines = package.Lines.OfType<ProjectCostLine>()
            .Where(l => l.Details?.CategoryId == ProjectCostCategories.WheelsAndAxles)
            .ToList();
        Assert.Single(waLines);
        Assert.Equal(ProjectCostItems.WaRental, waLines[0].Details!.ItemId);
        Assert.Equal(500m, waLines[0].SalePrice);
    }

    [Fact]
    public async Task Single_section_home_no_wa_when_option_null()
    {
        // Single-section home with no WheelAndAxlesOption → no W&A lines.
        var package = CreatePackageWithSaleContext();
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        var home = CreateHomeRequest(
            numberOfFloorSections: 1,
            wheelAndAxlesOption: null);

        var result = await _sut.Handle(
            new UpdatePackageHomeCommand(package.PublicId, home), CancellationToken.None);

        Assert.True(result.IsSuccess);

        var waLines = package.Lines.OfType<ProjectCostLine>()
            .Where(l => l.Details?.CategoryId == ProjectCostCategories.WheelsAndAxles);
        Assert.Empty(waLines);
    }

    [Fact]
    public async Task Multi_section_home_does_not_call_iseries_for_wa_pricing()
    {
        // Multi-section homes should short-circuit before calling iSeries for W&A pricing.
        var package = CreatePackageWithSaleContext();
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        var home = CreateHomeRequest(
            numberOfFloorSections: 3,
            wheelAndAxlesOption: WheelAndAxlesOption.Purchase,
            numberOfWheels: 6,
            numberOfAxles: 3);

        await _sut.Handle(
            new UpdatePackageHomeCommand(package.PublicId, home), CancellationToken.None);

        // iSeries should NOT be called for W&A pricing on multi-section homes
        await _iSeriesAdapter.DidNotReceive()
            .CalculateWheelAndAxlePriceByCount(Arg.Any<WheelAndAxlePriceByCountRequest>(), Arg.Any<CancellationToken>());
        await _iSeriesAdapter.DidNotReceive()
            .GetWheelAndAxlePriceByStock(Arg.Any<WheelAndAxlePriceByStockRequest>(), Arg.Any<CancellationToken>());
    }

    // --- Helpers ---

    private static UpdatePackageHomeRequest CreateHomeRequest(
        HomeSourceType homeSourceType = HomeSourceType.Quoted,
        HomeType homeType = HomeType.New,
        string? stockNumber = null,
        int? numberOfFloorSections = 2,
        WheelAndAxlesOption? wheelAndAxlesOption = null,
        int? numberOfWheels = null,
        int? numberOfAxles = null) =>
        new(
            SalePrice: 85000m,
            EstimatedCost: 60000m,
            RetailSalePrice: 90000m,
            StockNumber: stockNumber,
            HomeType: homeType,
            HomeSourceType: homeSourceType,
            ModularType: ModularType.Hud,
            Vendor: null, Make: null, Model: null, ModelYear: null,
            LengthInFeet: null, WidthInFeet: null, Bedrooms: null, Bathrooms: null,
            SquareFootage: null, SerialNumbers: null,
            BaseCost: null, OptionsCost: null, FreightCost: null, InvoiceCost: null,
            NetInvoice: null, GrossCost: null, TaxIncludedOnInvoice: null,
            NumberOfWheels: numberOfWheels, NumberOfAxles: numberOfAxles,
            WheelAndAxlesOption: wheelAndAxlesOption,
            NumberOfFloorSections: numberOfFloorSections, CarrierFrameDeposit: null,
            RebateOnMfgInvoice: null,
            ClaytonBuilt: null, BuildType: null, InventoryReferenceId: null,
            StateAssociationAndMhiDues: null, PartnerAssistance: null, DistanceMiles: null,
            PropertyType: null, PurchaseOption: null, ListingPrice: null,
            AccountNumber: null, DisplayAccountId: null,
            StreetAddress: null, City: null, State: null, ZipCode: null);

    private static Package CreatePackageWithSaleContext()
    {
        var sale = Sale.Create(customerId: 1, retailLocationId: 1, saleType: SaleType.B2C, saleNumber: 12345);
        sale.ClearDomainEvents();

        var retailLocation = RetailLocationCacheEntity.CreateHomeCenter(
            homeCenterNumber: 42, name: "Test HC", stateCode: "OH", zip: "43004", isActive: true);
        SetProperty(sale, nameof(Sale.RetailLocation), retailLocation);

        var package = Package.Create(saleId: sale.Id, name: "Primary", isPrimary: true);
        package.ClearDomainEvents();
        SetProperty(package, nameof(Package.Sale), sale);

        return package;
    }

    private static void SetProperty(object obj, string propertyName, object? value)
    {
        var backingField = obj.GetType().GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
        if (backingField is not null)
        {
            backingField.SetValue(obj, value);
        }
        else
        {
            var prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)!;
            prop.SetValue(obj, value);
        }
    }
}
