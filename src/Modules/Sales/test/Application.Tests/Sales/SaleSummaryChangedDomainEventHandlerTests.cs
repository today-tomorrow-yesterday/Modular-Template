using Modules.Sales.Application.Sales.EventHandlers;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Home;
using Modules.Sales.Domain.PartiesCache;
using Modules.Sales.Domain.RetailLocations;
using Modules.Sales.Domain.Sales;
using Modules.Sales.Domain.Sales.Events;
using Modules.Sales.IntegrationEvents;
using NSubstitute;
using Rtl.Core.Application.EventBus;
using System.Reflection;
using Xunit;

namespace Modules.Sales.Application.Tests.Sales;

public sealed class SaleSummaryChangedDomainEventHandlerTests
{
    private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
    private readonly IEventBus _eventBus = Substitute.For<IEventBus>();
    private readonly SaleSummaryChangedDomainEventHandler _sut;

    public SaleSummaryChangedDomainEventHandlerTests()
    {
        _sut = new SaleSummaryChangedDomainEventHandler(_saleRepository, _eventBus);
    }

    [Fact]
    public async Task Sale_not_found_does_nothing()
    {
        _saleRepository.GetByIdWithContextAsync(999, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);

        var domainEvent = new SaleSummaryChangedDomainEvent { SaleId = 999 };

        await _sut.Handle(domainEvent, CancellationToken.None);

        await _eventBus.DidNotReceive().PublishAsync(
            Arg.Any<SaleSummaryChangedIntegrationEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Publishes_integration_event_with_correct_stock_number()
    {
        var sale = CreateSaleWithContext(stockNumber: "STK-12345");
        _saleRepository.GetByIdWithContextAsync(sale.Id, Arg.Any<CancellationToken>())
            .Returns(sale);

        var domainEvent = new SaleSummaryChangedDomainEvent { SaleId = sale.Id };

        await _sut.Handle(domainEvent, CancellationToken.None);

        await _eventBus.Received(1).PublishAsync(
            Arg.Is<SaleSummaryChangedIntegrationEvent>(e =>
                e.StockNumber == "STK-12345" &&
                e.SaleId == sale.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Publishes_integration_event_with_correct_customer_name()
    {
        var sale = CreateSaleWithContext(customerName: "Jane Doe");
        _saleRepository.GetByIdWithContextAsync(sale.Id, Arg.Any<CancellationToken>())
            .Returns(sale);

        var domainEvent = new SaleSummaryChangedDomainEvent { SaleId = sale.Id };

        await _sut.Handle(domainEvent, CancellationToken.None);

        await _eventBus.Received(1).PublishAsync(
            Arg.Is<SaleSummaryChangedIntegrationEvent>(e =>
                e.CustomerName == "Jane Doe"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Publishes_integration_event_with_null_stock_number_when_no_home_line()
    {
        var sale = CreateSaleWithContext(includeHomeLine: false);
        _saleRepository.GetByIdWithContextAsync(sale.Id, Arg.Any<CancellationToken>())
            .Returns(sale);

        var domainEvent = new SaleSummaryChangedDomainEvent { SaleId = sale.Id };

        await _sut.Handle(domainEvent, CancellationToken.None);

        await _eventBus.Received(1).PublishAsync(
            Arg.Is<SaleSummaryChangedIntegrationEvent>(e =>
                e.StockNumber == null &&
                e.OriginalRetailPrice == null &&
                e.CurrentRetailPrice == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Publishes_integration_event_with_null_stock_number_when_no_primary_package()
    {
        var sale = CreateSaleWithContext(includePrimaryPackage: false);
        _saleRepository.GetByIdWithContextAsync(sale.Id, Arg.Any<CancellationToken>())
            .Returns(sale);

        var domainEvent = new SaleSummaryChangedDomainEvent { SaleId = sale.Id };

        await _sut.Handle(domainEvent, CancellationToken.None);

        await _eventBus.Received(1).PublishAsync(
            Arg.Is<SaleSummaryChangedIntegrationEvent>(e =>
                e.StockNumber == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Publishes_integration_event_with_pricing_from_home_line()
    {
        var sale = CreateSaleWithContext(
            salePrice: 85000m,
            retailSalePrice: 90000m);
        _saleRepository.GetByIdWithContextAsync(sale.Id, Arg.Any<CancellationToken>())
            .Returns(sale);

        var domainEvent = new SaleSummaryChangedDomainEvent { SaleId = sale.Id };

        await _sut.Handle(domainEvent, CancellationToken.None);

        await _eventBus.Received(1).PublishAsync(
            Arg.Is<SaleSummaryChangedIntegrationEvent>(e =>
                e.OriginalRetailPrice == 90000m &&
                e.CurrentRetailPrice == 85000m),
            Arg.Any<CancellationToken>());
    }

    // --- Test helpers ---

    private static Sale CreateSaleWithContext(
        string? stockNumber = "STK-001",
        string customerName = "Test Customer",
        bool includePrimaryPackage = true,
        bool includeHomeLine = true,
        decimal salePrice = 85000m,
        decimal retailSalePrice = 90000m)
    {
        var sale = Sale.Create(
            partyId: 1,
            retailLocationId: 1,
            saleType: SaleType.B2C,
            saleNumber: 12345);
        sale.ClearDomainEvents();

        // Set Party navigation via reflection
        var party = new PartyCache
        {
            Id = 1,
            RefPartyId = 1,
            RefPublicId = Guid.NewGuid(),
            DisplayName = customerName
        };
        SetProperty(sale, nameof(Sale.Party), party);

        // Set RetailLocation navigation via reflection
        var retailLocation = RetailLocation.CreateHomeCenter(
            homeCenterNumber: 42, name: "Test HC", stateCode: "OH", zip: "43004", isActive: true);
        SetProperty(sale, nameof(Sale.RetailLocation), retailLocation);

        if (includePrimaryPackage)
        {
            var package = Package.Create(saleId: sale.Id, name: "Primary", isPrimary: true);
            package.ClearDomainEvents();

            if (includeHomeLine)
            {
                var homeDetails = HomeDetails.Create(
                    homeType: HomeType.New,
                    homeSourceType: HomeSourceType.OnLot,
                    stockNumber: stockNumber);
                var homeLine = HomeLine.Create(
                    packageId: package.Id,
                    salePrice: salePrice,
                    estimatedCost: 60000m,
                    retailSalePrice: retailSalePrice,
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
        var backingField = obj.GetType().GetField($"<{propertyName}>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (backingField is not null)
        {
            backingField.SetValue(obj, value);
        }
        else
        {
            obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)!.SetValue(obj, value);
        }
    }
}
