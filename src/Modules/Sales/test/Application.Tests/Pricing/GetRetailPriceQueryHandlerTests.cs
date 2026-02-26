using Modules.Sales.Application.Pricing.GetRetailPrice;
using Modules.Sales.Domain.RetailLocations;
using Modules.Sales.Domain.Sales;
using NSubstitute;
using Rtl.Core.Application.Adapters.ISeries;
using Rtl.Core.Application.Adapters.ISeries.Pricing;
using System.Reflection;
using Xunit;

namespace Modules.Sales.Application.Tests.Pricing;

public sealed class GetRetailPriceQueryHandlerTests
{
    private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
    private readonly IiSeriesAdapter _iSeriesAdapter = Substitute.For<IiSeriesAdapter>();
    private readonly GetRetailPriceQueryHandler _sut;

    public GetRetailPriceQueryHandlerTests() =>
        _sut = new GetRetailPriceQueryHandler(_saleRepository, _iSeriesAdapter);

    [Fact]
    public async Task Returns_failure_when_sale_not_found()
    {
        var publicId = Guid.NewGuid();
        _saleRepository.GetByPublicIdWithPartyContextAsync(publicId, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);

        var result = await _sut.Handle(
            new GetRetailPriceQuery(publicId, "SN001", 50000m, 2, 1000m, 1500m, "MOD1", 40000m, "2025-01-01"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Sale.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Returns_success_with_retail_price()
    {
        var sale = CreateSaleWithRetailLocation("OH");
        _saleRepository.GetByPublicIdWithPartyContextAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);

        _iSeriesAdapter.CalculateRetailPrice(Arg.Any<RetailPriceRequest>(), Arg.Any<CancellationToken>())
            .Returns(75000m);

        var result = await _sut.Handle(
            new GetRetailPriceQuery(sale.PublicId, "SN001", 50000m, 2, 1000m, 1500m, "MOD1", 40000m, "2025-01-01"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(75000m, result.Value);
    }

    private static Sale CreateSaleWithRetailLocation(string stateCode)
    {
        var sale = Sale.Create(partyId: 1, retailLocationId: 1, saleType: SaleType.B2C, saleNumber: 100);
        sale.ClearDomainEvents();

        var retailLocation = RetailLocation.CreateHomeCenter(
            homeCenterNumber: 42, name: "Test HC", stateCode: stateCode, zip: "43004", isActive: true);
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
