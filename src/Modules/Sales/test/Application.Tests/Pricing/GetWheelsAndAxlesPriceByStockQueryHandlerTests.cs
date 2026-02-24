using System.Reflection;
using Modules.Sales.Application.Pricing.GetWheelsAndAxlesPriceByStock;
using Modules.Sales.Domain.RetailLocations;
using Modules.Sales.Domain.Sales;
using NSubstitute;
using Rtl.Core.Application.Adapters.ISeries;
using Rtl.Core.Application.Adapters.ISeries.Pricing;
using Xunit;

namespace Modules.Sales.Application.Tests.Pricing;

public sealed class GetWheelsAndAxlesPriceByStockQueryHandlerTests
{
    private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
    private readonly IiSeriesAdapter _iSeriesAdapter = Substitute.For<IiSeriesAdapter>();
    private readonly GetWheelsAndAxlesPriceByStockQueryHandler _sut;

    public GetWheelsAndAxlesPriceByStockQueryHandlerTests() =>
        _sut = new GetWheelsAndAxlesPriceByStockQueryHandler(_saleRepository, _iSeriesAdapter);

    [Fact]
    public async Task Returns_failure_when_sale_not_found()
    {
        var publicId = Guid.NewGuid();
        _saleRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);

        var result = await _sut.Handle(
            new GetWheelsAndAxlesPriceByStockQuery(publicId, "STK001"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Sale.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Returns_success_with_price()
    {
        var sale = CreateSaleWithRetailLocation();
        _saleRepository.GetByPublicIdAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);

        _iSeriesAdapter.CalculateWheelAndAxlePrice(Arg.Any<WheelAndAxlePriceByStockRequest>(), Arg.Any<CancellationToken>())
            .Returns(3200m);

        var result = await _sut.Handle(
            new GetWheelsAndAxlesPriceByStockQuery(sale.PublicId, "STK001"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3200m, result.Value);
    }

    private static Sale CreateSaleWithRetailLocation()
    {
        var sale = Sale.Create(partyId: 1, retailLocationId: 1, saleType: SaleType.B2C, saleNumber: 100);
        sale.ClearDomainEvents();

        var retailLocation = RetailLocation.CreateHomeCenter(
            homeCenterNumber: 42, name: "Test HC", stateCode: "OH", zip: "43004", isActive: true);
        SetProperty(sale, nameof(Sale.RetailLocation), retailLocation);

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
            obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)!
                .SetValue(obj, value);
        }
    }
}
