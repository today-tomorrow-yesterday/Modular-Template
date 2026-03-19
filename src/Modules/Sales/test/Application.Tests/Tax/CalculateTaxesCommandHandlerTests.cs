using Modules.Sales.Application.Tax.CalculateTaxes;
using Modules.Sales.Domain;
using Modules.Sales.Domain.FundingCache;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Home;
using Modules.Sales.Domain.Packages.Tax;
using Modules.Sales.Domain.RetailLocations;
using Modules.Sales.Domain.Sales;
using NSubstitute;
using Rtl.Core.Application.Adapters.ISeries;
using Rtl.Core.Application.Adapters.ISeries.Tax;
using Rtl.Core.Application.Persistence;
using System.Reflection;
using System.Text.Json;
using Xunit;
using DomainDeliveryAddress = Modules.Sales.Domain.DeliveryAddresses.DeliveryAddress;

namespace Modules.Sales.Application.Tests.Tax;

public sealed class CalculateTaxesCommandHandlerTests
{
    private readonly IPackageRepository _packageRepository = Substitute.For<IPackageRepository>();
    private readonly IFundingRequestCacheRepository _fundingRequestCacheRepository = Substitute.For<IFundingRequestCacheRepository>();
    private readonly IiSeriesAdapter _iSeriesAdapter = Substitute.For<IiSeriesAdapter>();
    private readonly IUnitOfWork<ISalesModule> _unitOfWork = Substitute.For<IUnitOfWork<ISalesModule>>();
    private readonly CalculateTaxesCommandHandler _sut;

    public CalculateTaxesCommandHandlerTests() =>
        _sut = new CalculateTaxesCommandHandler(
            _packageRepository, _fundingRequestCacheRepository, _iSeriesAdapter, _unitOfWork);

    [Fact]
    public async Task Returns_failure_when_package_not_found()
    {
        var publicId = Guid.NewGuid();
        _packageRepository.GetByPublicIdWithSaleContextAsync(publicId, Arg.Any<CancellationToken>())
            .Returns((Package?)null);

        var result = await _sut.Handle(new CalculateTaxesCommand(publicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Package.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Returns_failure_when_no_home_line()
    {
        var package = CreatePackageWithContext(includeHomeLine: false);
        SetupPackageRepo(package);

        var result = await _sut.Handle(new CalculateTaxesCommand(package.PublicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Tax.NoHomeLine", result.Error.Code);
    }

    [Fact]
    public async Task Returns_failure_when_no_delivery_address()
    {
        var package = CreatePackageWithContext(includeDeliveryAddress: false);
        SetupPackageRepo(package);

        var result = await _sut.Handle(new CalculateTaxesCommand(package.PublicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Tax.NoDeliveryAddress", result.Error.Code);
    }

    [Fact]
    public async Task Returns_failure_when_no_retail_location()
    {
        var package = CreatePackageWithContext(includeRetailLocation: false);
        SetupPackageRepo(package);

        var result = await _sut.Handle(new CalculateTaxesCommand(package.PublicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Tax.NoRetailLocation", result.Error.Code);
    }

    [Fact]
    public async Task Returns_failure_when_no_previously_titled()
    {
        var package = CreatePackageWithContext(includeTaxLine: false);
        SetupPackageRepo(package);

        var result = await _sut.Handle(new CalculateTaxesCommand(package.PublicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Tax.NoPreviouslyTitled", result.Error.Code);
    }

    [Fact]
    public async Task Returns_failure_when_no_app_id_in_funding_cache()
    {
        var package = CreatePackageWithContext();
        SetupPackageRepo(package);
        _fundingRequestCacheRepository.GetByPackageIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((FundingRequestCache?)null);

        var result = await _sut.Handle(new CalculateTaxesCommand(package.PublicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Tax.NoAppId", result.Error.Code);
    }

    [Fact]
    public async Task Returns_success_on_happy_path()
    {
        var package = CreatePackageWithContext();
        SetupPackageRepo(package);
        SetupFundingCache(999999);
        _iSeriesAdapter.CalculateTax(Arg.Any<TaxCalculationRequest>(), Arg.Any<CancellationToken>())
            .Returns(new TaxCalculationResult
            {
                StateTax = 100m,
                CityTax = 50m,
                CountyTax = 25m,
                UseTax = 0m,
                Messages = null
            });

        var result = await _sut.Handle(new CalculateTaxesCommand(package.PublicId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(175m, result.Value.TaxSalePrice);
        Assert.Equal(6, result.Value.TaxItems.Count);
        Assert.Empty(result.Value.Errors);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Returns_success_with_errors_when_iseries_returns_messages()
    {
        var package = CreatePackageWithContext();
        SetupPackageRepo(package);
        SetupFundingCache(999999);
        _iSeriesAdapter.CalculateTax(Arg.Any<TaxCalculationRequest>(), Arg.Any<CancellationToken>())
            .Returns(new TaxCalculationResult
            {
                StateTax = 0m,
                CityTax = 0m,
                CountyTax = 0m,
                UseTax = 0m,
                Messages = ["Tax calculation error from iSeries"]
            });

        var result = await _sut.Handle(new CalculateTaxesCommand(package.PublicId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0m, result.Value.TaxSalePrice);
        Assert.Single(result.Value.Errors);
    }

    // --- Helpers ---

    private void SetupPackageRepo(Package package) =>
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

    private void SetupFundingCache(int appId) =>
        _fundingRequestCacheRepository.GetByPackageIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new FundingRequestCache
            {
                Id = 1,
                RefFundingRequestId = 100,
                SaleId = 0,
                PackageId = 0,
                FundingKeys = JsonDocument.Parse($$"""[{"Key":"AppId","Value":"{{appId}}"}]"""),
                LenderId = 1,
                LenderName = "Test Lender",
                RequestAmount = 85000m,
                Status = FundingRequestStatus.Approved,
                LastSyncedAtUtc = DateTime.UtcNow
            });

    private static Package CreatePackageWithContext(
        bool includeHomeLine = true,
        bool includeDeliveryAddress = true,
        bool includeRetailLocation = true,
        bool includeTaxLine = true)
    {
        var sale = Sale.Create(
            customerId: 1,
            retailLocationId: 1,
            saleType: SaleType.B2C,
            saleNumber: 12345);
        sale.ClearDomainEvents();

        if (includeRetailLocation)
        {
            var retailLocation = RetailLocation.CreateHomeCenter(
                homeCenterNumber: 42, name: "Test HC", stateCode: "OH", zip: "43004", isActive: true);
            SetProperty(sale, nameof(Sale.RetailLocation), retailLocation);
        }

        if (includeDeliveryAddress)
        {
            var address = DomainDeliveryAddress.Create(
                saleId: sale.Id,
                occupancyType: "Primary",
                isWithinCityLimits: true,
                addressStyle: null, addressType: null,
                addressLine1: "123 Main St", addressLine2: null, addressLine3: null,
                city: "Columbus", county: "Franklin",
                state: "OH", country: "US", postalCode: "43004");
            address.ClearDomainEvents();
            SetProperty(sale, nameof(Sale.DeliveryAddress), address);
        }

        var package = Package.Create(saleId: sale.Id, name: "Primary", isPrimary: true);
        package.ClearDomainEvents();

        if (includeHomeLine)
        {
            var homeDetails = HomeDetails.Create(
                homeType: HomeType.New,
                homeSourceType: HomeSourceType.OnLot,
                modularType: ModularType.Hud,
                stockNumber: "STK-001",
                netInvoice: 55000m,
                numberOfFloorSections: 2,
                freightCost: 3000m,
                carrierFrameDeposit: 500m,
                grossCost: 58000m,
                taxIncludedOnInvoice: 0m,
                rebateOnMfgInvoice: 0m);
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

        if (includeTaxLine)
        {
            var taxDetails = TaxDetails.Create(
                previouslyTitled: "Yes",
                taxExemptionId: null,
                questionAnswers: [],
                taxes: [],
                errors: null);
            var taxLine = TaxLine.Create(
                packageId: package.Id,
                salePrice: 0m,
                estimatedCost: 0m,
                retailSalePrice: 0m,
                shouldExcludeFromPricing: false,
                details: taxDetails);
            package.AddLine(taxLine);
            package.ClearDomainEvents();
        }

        SetProperty(package, nameof(Package.Sale), sale);

        var packagesField = typeof(Sale).GetField("_packages", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var packages = (List<Package>)packagesField.GetValue(sale)!;
        packages.Add(package);

        return package;
    }

    private static void SetProperty(object obj, string propertyName, object? value)
    {
        var backingField = obj.GetType().GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
        if (backingField is not null)
            backingField.SetValue(obj, value);
        else
            obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)!.SetValue(obj, value);
    }
}
