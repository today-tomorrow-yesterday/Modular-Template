using Modules.Sales.Application.Packages.UpdatePackageHome;
using Modules.Sales.Domain;
using Modules.Sales.Domain.InventoryCache;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Details;
using Modules.Sales.Domain.RetailLocations;
using Modules.Sales.Domain.Sales;
using NSubstitute;
using Rtl.Core.Application.Adapters.ISeries;
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
    private readonly UpdatePackageHomeCommandHandler _sut;

    public UpdatePackageHomeCommandHandlerTests()
    {
        _sut = new UpdatePackageHomeCommandHandler(
            _packageRepository, _inventoryCacheQueries, _iSeriesAdapter, _unitOfWork);
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

    // --- Helpers ---

    private static UpdatePackageHomeRequest CreateHomeRequest(
        HomeSourceType homeSourceType = HomeSourceType.Quoted,
        HomeType homeType = HomeType.New,
        string? stockNumber = null) =>
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
            NumberOfWheels: null, NumberOfAxles: null, WheelAndAxlesOption: null,
            NumberOfFloorSections: 2, CarrierFrameDeposit: null, RebateOnMfgInvoice: null,
            ClaytonBuilt: null, BuildType: null, InventoryReferenceId: null,
            StateAssociationAndMhiDues: null, PartnerAssistance: null, DistanceMiles: null,
            PropertyType: null, PurchaseOption: null, ListingPrice: null,
            AccountNumber: null, DisplayAccountId: null,
            StreetAddress: null, City: null, State: null, ZipCode: null);

    private static Package CreatePackageWithSaleContext()
    {
        var sale = Sale.Create(partyId: 1, retailLocationId: 1, saleType: SaleType.B2C, saleNumber: 12345);
        sale.ClearDomainEvents();

        var retailLocation = RetailLocation.CreateHomeCenter(
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
