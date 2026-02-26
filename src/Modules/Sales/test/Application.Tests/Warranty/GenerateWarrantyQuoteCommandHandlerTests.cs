using Modules.Sales.Application.Insurance.GenerateWarrantyQuote;
using Modules.Sales.Domain;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Home;
using Modules.Sales.Domain.Packages.Warranty;
using Modules.Sales.Domain.RetailLocations;
using Modules.Sales.Domain.Sales;
using NSubstitute;
using Rtl.Core.Application.Adapters.ISeries;
using Rtl.Core.Application.Adapters.ISeries.Insurance;
using Rtl.Core.Application.Persistence;
using System.Reflection;
using Xunit;
using DomainDeliveryAddress = Modules.Sales.Domain.DeliveryAddresses.DeliveryAddress;

namespace Modules.Sales.Application.Tests.Warranty;

public sealed class GenerateWarrantyQuoteCommandHandlerTests
{
    private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
    private readonly IiSeriesAdapter _iSeriesAdapter = Substitute.For<IiSeriesAdapter>();
    private readonly IUnitOfWork<ISalesModule> _unitOfWork = Substitute.For<IUnitOfWork<ISalesModule>>();
    private readonly GenerateWarrantyQuoteCommandHandler _sut;

    public GenerateWarrantyQuoteCommandHandlerTests() =>
        _sut = new GenerateWarrantyQuoteCommandHandler(_saleRepository, _iSeriesAdapter, _unitOfWork);

    [Fact]
    public async Task Returns_failure_when_sale_not_found()
    {
        var publicId = Guid.NewGuid();
        _saleRepository.GetByPublicIdWithFullContextAsync(publicId, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);

        var result = await _sut.Handle(
            new GenerateWarrantyQuoteCommand(publicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Sale.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Returns_failure_when_no_delivery_address()
    {
        var sale = CreateSaleWithContext(includeDeliveryAddress: false);
        _saleRepository.GetByPublicIdWithFullContextAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);

        var result = await _sut.Handle(
            new GenerateWarrantyQuoteCommand(sale.PublicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Warranty.NoDeliveryAddress", result.Error.Code);
    }

    [Fact]
    public async Task Returns_failure_when_no_primary_package()
    {
        var sale = CreateSaleWithContext(includePrimaryPackage: false);
        _saleRepository.GetByPublicIdWithFullContextAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);

        var result = await _sut.Handle(
            new GenerateWarrantyQuoteCommand(sale.PublicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Warranty.NoPrimaryPackage", result.Error.Code);
    }

    [Fact]
    public async Task Returns_failure_when_no_home_line()
    {
        var sale = CreateSaleWithContext(includeHomeLine: false);
        _saleRepository.GetByPublicIdWithFullContextAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);

        var result = await _sut.Handle(
            new GenerateWarrantyQuoteCommand(sale.PublicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Warranty.NoHomeLine", result.Error.Code);
    }

    [Fact]
    public async Task Returns_failure_when_home_details_missing_required_fields()
    {
        var sale = CreateSaleWithContext(homeModelYear: null, homeModularType: null);
        _saleRepository.GetByPublicIdWithFullContextAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);

        var result = await _sut.Handle(
            new GenerateWarrantyQuoteCommand(sale.PublicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Warranty.MissingHomeDetails", result.Error.Code);
    }

    [Theory]
    [InlineData("Rental")]
    [InlineData("Investment")]
    public async Task Succeeds_regardless_of_occupancy_type(string occupancyType)
    {
        var sale = CreateSaleWithContext(occupancyType: occupancyType);
        _saleRepository.GetByPublicIdWithFullContextAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);
        _iSeriesAdapter.CalculateWarrantyQuote(Arg.Any<WarrantyQuoteRequest>(), Arg.Any<CancellationToken>())
            .Returns(new WarrantyQuoteResult { Premium = 875.00m, SalesTaxPremium = 72.19m });

        var result = await _sut.Handle(
            new GenerateWarrantyQuoteCommand(sale.PublicId), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Successful_quote_returns_premium_and_tax()
    {
        var sale = CreateSaleWithContext();
        _saleRepository.GetByPublicIdWithFullContextAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);
        _iSeriesAdapter.CalculateWarrantyQuote(Arg.Any<WarrantyQuoteRequest>(), Arg.Any<CancellationToken>())
            .Returns(new WarrantyQuoteResult { Premium = 875.00m, SalesTaxPremium = 72.19m });

        var result = await _sut.Handle(
            new GenerateWarrantyQuoteCommand(sale.PublicId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(875.00m, result.Value.Premium);
        Assert.Equal(72.19m, result.Value.SalesTaxPremium);
        Assert.True(result.Value.WarrantySelected);
    }

    [Fact]
    public async Task Successful_quote_creates_warranty_line_on_primary_package()
    {
        var sale = CreateSaleWithContext();
        _saleRepository.GetByPublicIdWithFullContextAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);
        _iSeriesAdapter.CalculateWarrantyQuote(Arg.Any<WarrantyQuoteRequest>(), Arg.Any<CancellationToken>())
            .Returns(new WarrantyQuoteResult { Premium = 875.00m, SalesTaxPremium = 72.19m });

        await _sut.Handle(
            new GenerateWarrantyQuoteCommand(sale.PublicId), CancellationToken.None);

        var primaryPackage = sale.Packages.First(p => p.IsPrimaryPackage);
        var warranty = Assert.Single(primaryPackage.Lines.OfType<WarrantyLine>());
        Assert.Equal(875.00m, warranty.SalePrice);
        Assert.NotNull(warranty.Details);
        Assert.True(warranty.Details.WarrantySelected);
        Assert.Equal(875.00m, warranty.Details.WarrantyAmount);
        Assert.Equal(72.19m, warranty.Details.SalesTaxPremium);
    }

    [Fact]
    public async Task Replaces_existing_warranty_line_with_new_quote()
    {
        var sale = CreateSaleWithContext(includeExistingWarranty: true);
        _saleRepository.GetByPublicIdWithFullContextAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);
        _iSeriesAdapter.CalculateWarrantyQuote(Arg.Any<WarrantyQuoteRequest>(), Arg.Any<CancellationToken>())
            .Returns(new WarrantyQuoteResult { Premium = 950.00m, SalesTaxPremium = 78.38m });

        await _sut.Handle(
            new GenerateWarrantyQuoteCommand(sale.PublicId), CancellationToken.None);

        var primaryPackage = sale.Packages.First(p => p.IsPrimaryPackage);
        var warranty = Assert.Single(primaryPackage.Lines.OfType<WarrantyLine>());
        Assert.Equal(950.00m, warranty.SalePrice);
        Assert.Equal(950.00m, warranty.Details!.WarrantyAmount);
    }

    [Fact]
    public async Task Flags_tax_recalculation_when_premium_changes()
    {
        var sale = CreateSaleWithContext(includeExistingWarranty: true);
        var primaryPackage = sale.Packages.First(p => p.IsPrimaryPackage);
        primaryPackage.ClearTaxRecalculationFlag();

        _saleRepository.GetByPublicIdWithFullContextAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);
        _iSeriesAdapter.CalculateWarrantyQuote(Arg.Any<WarrantyQuoteRequest>(), Arg.Any<CancellationToken>())
            .Returns(new WarrantyQuoteResult { Premium = 999.00m, SalesTaxPremium = 82.00m });

        await _sut.Handle(
            new GenerateWarrantyQuoteCommand(sale.PublicId), CancellationToken.None);

        Assert.True(primaryPackage.MustRecalculateTaxes);
    }

    [Fact]
    public async Task Calls_save_changes()
    {
        var sale = CreateSaleWithContext();
        _saleRepository.GetByPublicIdWithFullContextAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);
        _iSeriesAdapter.CalculateWarrantyQuote(Arg.Any<WarrantyQuoteRequest>(), Arg.Any<CancellationToken>())
            .Returns(new WarrantyQuoteResult { Premium = 875.00m, SalesTaxPremium = 72.19m });

        await _sut.Handle(
            new GenerateWarrantyQuoteCommand(sale.PublicId), CancellationToken.None);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Passes_correct_adapter_request_fields()
    {
        var sale = CreateSaleWithContext();
        _saleRepository.GetByPublicIdWithFullContextAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);
        _iSeriesAdapter.CalculateWarrantyQuote(Arg.Any<WarrantyQuoteRequest>(), Arg.Any<CancellationToken>())
            .Returns(new WarrantyQuoteResult { Premium = 875.00m, SalesTaxPremium = 72.19m });

        await _sut.Handle(
            new GenerateWarrantyQuoteCommand(sale.PublicId), CancellationToken.None);

        await _iSeriesAdapter.Received(1).CalculateWarrantyQuote(
            Arg.Is<WarrantyQuoteRequest>(r =>
                r.HomeCenterNumber == 42 &&
                r.AppId == 0 && // Legacy hardcodes 0 — warranty quote is stateless
                r.PhysicalState == "OH" &&
                r.PhysicalZip == "43004" &&
                r.WidthInFeet == 0 &&
                r.HomeCondition == HomeCondition.New &&
                r.ModularClassification == ModularClassification.Hud &&
                r.CalculateSalesTax == true),
            Arg.Any<CancellationToken>());
    }

    // --- Test helpers ---

    private static Sale CreateSaleWithContext(
        bool includeDeliveryAddress = true,
        bool includePrimaryPackage = true,
        bool includeHomeLine = true,
        bool includeExistingWarranty = false,
        int? homeModelYear = 2024,
        ModularType? homeModularType = ModularType.Hud,
        string? occupancyType = "Primary")
    {
        var sale = Sale.Create(
            partyId: 1,
            retailLocationId: 1,
            saleType: SaleType.B2C,
            saleNumber: 12345);
        sale.ClearDomainEvents();

        // Set RetailLocation via reflection (normally populated by EF Core Include)
        var retailLocation = RetailLocation.CreateHomeCenter(
            homeCenterNumber: 42, name: "Test HC", stateCode: "OH", zip: "43004", isActive: true);
        SetProperty(sale, nameof(Sale.RetailLocation), retailLocation);

        if (includeDeliveryAddress)
        {
            var address = DomainDeliveryAddress.Create(
                saleId: sale.Id,
                occupancyType: occupancyType,
                isWithinCityLimits: true,
                addressStyle: null, addressType: null,
                addressLine1: "123 Main St", addressLine2: null, addressLine3: null,
                city: "Columbus", county: "Franklin",
                state: "OH", country: "US", postalCode: "43004");
            address.ClearDomainEvents();
            SetProperty(sale, nameof(Sale.DeliveryAddress), address);
        }

        if (includePrimaryPackage)
        {
            var package = Package.Create(saleId: sale.Id, name: "Primary", isPrimary: true);
            package.ClearDomainEvents();

            if (includeHomeLine)
            {
                var homeDetails = HomeDetails.Create(
                    homeType: HomeType.New,
                    homeSourceType: HomeSourceType.OnLot,
                    modularType: homeModularType,
                    modelYear: homeModelYear,
                    numberOfFloorSections: 2);
                var homeLine = HomeLine.Create(
                    packageId: package.Id,
                    salePrice: 85000m,
                    estimatedCost: 60000m,
                    retailSalePrice: 90000m,
                    responsibility: null,
                    details: homeDetails);
                package.AddLine(homeLine);
                package.ClearDomainEvents();
            }

            if (includeExistingWarranty)
            {
                var warrantyDetails = WarrantyDetails.Create(800.00m, 66.00m);
                var warrantyLine = WarrantyLine.Create(
                    packageId: package.Id,
                    salePrice: 800.00m,
                    estimatedCost: 0m,
                    retailSalePrice: 0m,
                    shouldExcludeFromPricing: false,
                    details: warrantyDetails);
                package.AddLine(warrantyLine);
                package.ClearDomainEvents();
            }

            // Add package to sale's private _packages list via reflection
            var packagesField = typeof(Sale).GetField("_packages", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var packages = (List<Package>)packagesField.GetValue(sale)!;
            packages.Add(package);
        }

        return sale;
    }

    private static void SetProperty(object obj, string propertyName, object? value)
    {
        var prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)!;
        var backingField = obj.GetType().GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
        if (backingField is not null)
        {
            backingField.SetValue(obj, value);
        }
        else
        {
            // Try setter directly (may be private)
            prop.SetValue(obj, value);
        }
    }
}
