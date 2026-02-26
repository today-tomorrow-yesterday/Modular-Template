using Modules.Sales.Application.Insurance.GenerateHomeFirstQuote;
using Modules.Sales.Domain;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Home;
using Modules.Sales.Domain.Packages.Insurance;
using Modules.Sales.Domain.PartiesCache;
using Modules.Sales.Domain.RetailLocations;
using Modules.Sales.Domain.Sales;
using NSubstitute;
using Rtl.Core.Application.Adapters.ISeries;
using Rtl.Core.Application.Persistence;
using System.Reflection;
using Xunit;
using DomainDeliveryAddress = Modules.Sales.Domain.DeliveryAddresses.DeliveryAddress;
using ISeriesInsurance = Rtl.Core.Application.Adapters.ISeries.Insurance;

namespace Modules.Sales.Application.Tests.Insurance;

public sealed class GenerateHomeFirstQuoteCommandHandlerTests
{
    private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
    private readonly IiSeriesAdapter _iSeriesAdapter = Substitute.For<IiSeriesAdapter>();
    private readonly IUnitOfWork<ISalesModule> _unitOfWork = Substitute.For<IUnitOfWork<ISalesModule>>();
    private readonly GenerateHomeFirstQuoteCommandHandler _sut;

    public GenerateHomeFirstQuoteCommandHandlerTests() =>
        _sut = new GenerateHomeFirstQuoteCommandHandler(_saleRepository, _iSeriesAdapter, _unitOfWork);

    private static GenerateHomeFirstQuoteCommand CreateCommand(Guid salePublicId) =>
        new(salePublicId,
            CoverageAmount: 100000m,
            OccupancyType: 'P',
            IsHomeLocatedInPark: false,
            IsLandCustomerOwned: true,
            IsHomeOnPermanentFoundation: false,
            IsPremiumFinanced: true,
            CustomerBirthDate: new DateTime(1985, 6, 15),
            CoApplicantBirthDate: null,
            MailingAddress: "123 Main St",
            MailingCity: "Columbus",
            MailingState: "OH",
            MailingZip: "43004");

    [Fact]
    public async Task Returns_failure_when_sale_not_found()
    {
        var publicId = Guid.NewGuid();
        _saleRepository.GetByPublicIdWithFullContextAsync(publicId, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);

        var result = await _sut.Handle(CreateCommand(publicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Sale.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Returns_failure_when_no_delivery_address()
    {
        var sale = CreateSaleWithContext(includeDeliveryAddress: false);
        _saleRepository.GetByPublicIdWithFullContextAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);

        var result = await _sut.Handle(CreateCommand(sale.PublicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Insurance.NoDeliveryAddress", result.Error.Code);
    }

    [Fact]
    public async Task Returns_failure_when_no_primary_package()
    {
        var sale = CreateSaleWithContext(includePrimaryPackage: false);
        _saleRepository.GetByPublicIdWithFullContextAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);

        var result = await _sut.Handle(CreateCommand(sale.PublicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Insurance.NoPrimaryPackage", result.Error.Code);
    }

    [Fact]
    public async Task Returns_failure_when_no_home_line()
    {
        var sale = CreateSaleWithContext(includeHomeLine: false);
        _saleRepository.GetByPublicIdWithFullContextAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);

        var result = await _sut.Handle(CreateCommand(sale.PublicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Insurance.NoHomeLine", result.Error.Code);
    }

    [Fact]
    public async Task Returns_failure_when_occupancy_is_ineligible()
    {
        var sale = CreateSaleWithContext(occupancyType: "Rental");
        _saleRepository.GetByPublicIdWithFullContextAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);

        var result = await _sut.Handle(CreateCommand(sale.PublicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Insurance.IneligibleOccupancy", result.Error.Code);
    }

    [Fact]
    public async Task Returns_ineligible_result_when_iseries_returns_error()
    {
        var sale = CreateSaleWithContext();
        _saleRepository.GetByPublicIdWithFullContextAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);
        _iSeriesAdapter.CalculateHomeFirstQuote(Arg.Any<ISeriesInsurance.HomeFirstQuoteRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ISeriesInsurance.HomeFirstQuoteResult
            {
                TempLinkId = 0,
                InsuranceCompanyName = "HomeFirst",
                TotalPremium = 0m,
                MaximumCoverage = 0m,
                ErrorMessage = "Not eligible for coverage"
            });

        var result = await _sut.Handle(CreateCommand(sale.PublicId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsEligible);
        Assert.Equal("Not eligible for coverage", result.Value.ErrorMessage);
    }

    [Fact]
    public async Task Returns_success_on_happy_path()
    {
        var sale = CreateSaleWithContext();
        _saleRepository.GetByPublicIdWithFullContextAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);
        _iSeriesAdapter.CalculateHomeFirstQuote(Arg.Any<ISeriesInsurance.HomeFirstQuoteRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ISeriesInsurance.HomeFirstQuoteResult
            {
                TempLinkId = 42,
                InsuranceCompanyName = "HomeFirst Insurance Co",
                TotalPremium = 1500m,
                MaximumCoverage = 250000m,
                ErrorMessage = null
            });

        var result = await _sut.Handle(CreateCommand(sale.PublicId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsEligible);
        Assert.Equal(1500m, result.Value.Premium);
        Assert.Equal(42, result.Value.TempLinkId);

        var package = sale.Packages.First(p => p.IsPrimaryPackage);
        var insuranceLine = Assert.Single(package.Lines.OfType<InsuranceLine>());
        Assert.Equal(1500m, insuranceLine.SalePrice);
        Assert.Equal(InsuranceType.HomeFirst, insuranceLine.Details!.InsuranceType);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // --- Test helpers ---

    private static Sale CreateSaleWithContext(
        bool includeDeliveryAddress = true,
        bool includePrimaryPackage = true,
        bool includeHomeLine = true,
        string occupancyType = "Primary")
    {
        var sale = Sale.Create(
            partyId: 1,
            retailLocationId: 1,
            saleType: SaleType.B2C,
            saleNumber: 12345);
        sale.ClearDomainEvents();

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

        var party = new PartyCache
        {
            Id = 1,
            RefPartyId = 1,
            RefPublicId = Guid.NewGuid(),
            PartyType = PartyType.Person,
            LifecycleStage = LifecycleStage.Customer,
            HomeCenterNumber = 42,
            DisplayName = "John Doe",
            LastSyncedAtUtc = DateTime.UtcNow,
            Person = new PartyPersonCache
            {
                PartyId = 1,
                FirstName = "John",
                LastName = "Doe",
                Phone = "555-123-4567"
            }
        };
        SetProperty(sale, nameof(Sale.Party), party);

        if (includePrimaryPackage)
        {
            var package = Package.Create(saleId: sale.Id, name: "Primary", isPrimary: true);
            package.ClearDomainEvents();

            if (includeHomeLine)
            {
                var homeDetails = HomeDetails.Create(
                    homeType: HomeType.New,
                    homeSourceType: HomeSourceType.OnLot,
                    modularType: ModularType.Hud,
                    stockNumber: "STK-001",
                    model: "Model-A",
                    modelYear: 2024,
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

            var packagesField = typeof(Sale).GetField("_packages", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var packages = (List<Package>)packagesField.GetValue(sale)!;
            packages.Add(package);
        }

        return sale;
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
