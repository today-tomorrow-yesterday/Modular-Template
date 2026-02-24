using System.Reflection;
using Modules.Sales.Application.Pricing.GetOptionTotals;
using Modules.Sales.Domain.RetailLocations;
using Modules.Sales.Domain.Sales;
using NSubstitute;
using Rtl.Core.Application.Adapters.ISeries;
using Rtl.Core.Application.Adapters.ISeries.Pricing;
using Xunit;

namespace Modules.Sales.Application.Tests.Pricing;

public sealed class GetOptionTotalsQueryHandlerTests
{
    private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
    private readonly IiSeriesAdapter _iSeriesAdapter = Substitute.For<IiSeriesAdapter>();
    private readonly GetOptionTotalsQueryHandler _sut;

    public GetOptionTotalsQueryHandlerTests() =>
        _sut = new GetOptionTotalsQueryHandler(_saleRepository, _iSeriesAdapter);

    [Fact]
    public async Task Returns_failure_when_sale_not_found()
    {
        var publicId = Guid.NewGuid();
        _saleRepository.GetByPublicIdWithPartyContextAsync(publicId, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);

        var result = await _sut.Handle(
            new GetOptionTotalsQuery(publicId, 1, 100, 200, "2025-01-01"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Sale.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Returns_success_with_option_totals()
    {
        var sale = CreateSaleWithRetailLocation("OH");
        _saleRepository.GetByPublicIdWithPartyContextAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);

        _iSeriesAdapter.CalculateOptionTotals(Arg.Any<OptionTotalsRequest>(), Arg.Any<CancellationToken>())
            .Returns(new OptionTotalsResult { FactoryOptionTotal = 1000m, RetailOptionTotal = 1500m });

        var result = await _sut.Handle(
            new GetOptionTotalsQuery(sale.PublicId, 1, 100, 200, "2025-01-01"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1000m, result.Value.HbgOptionTotal);
        Assert.Equal(1500m, result.Value.RetailOptionTotal);
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
