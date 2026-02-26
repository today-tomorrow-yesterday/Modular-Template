using Modules.Sales.Application.Pricing.GetWheelsAndAxlesPrice;
using Modules.Sales.Domain.Sales;
using NSubstitute;
using Rtl.Core.Application.Adapters.ISeries;
using Rtl.Core.Application.Adapters.ISeries.Pricing;
using Xunit;

namespace Modules.Sales.Application.Tests.Pricing;

public sealed class GetWheelsAndAxlesPriceQueryHandlerTests
{
    private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
    private readonly IiSeriesAdapter _iSeriesAdapter = Substitute.For<IiSeriesAdapter>();
    private readonly GetWheelsAndAxlesPriceQueryHandler _sut;

    public GetWheelsAndAxlesPriceQueryHandlerTests() =>
        _sut = new GetWheelsAndAxlesPriceQueryHandler(_saleRepository, _iSeriesAdapter);

    [Fact]
    public async Task Returns_failure_when_sale_not_found()
    {
        var publicId = Guid.NewGuid();
        _saleRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);

        var result = await _sut.Handle(
            new GetWheelsAndAxlesPriceQuery(publicId, 4, 2), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Sale.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Returns_success_with_price()
    {
        var sale = Sale.Create(partyId: 1, retailLocationId: 1, saleType: SaleType.B2C, saleNumber: 100);
        sale.ClearDomainEvents();

        _saleRepository.GetByPublicIdAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);

        _iSeriesAdapter.CalculateWheelAndAxlePriceByCount(Arg.Any<WheelAndAxlePriceByCountRequest>(), Arg.Any<CancellationToken>())
            .Returns(new WheelAndAxlePriceResult(2500m, 2000m));

        var result = await _sut.Handle(
            new GetWheelsAndAxlesPriceQuery(sale.PublicId, 4, 2), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2500m, result.Value);
    }
}
