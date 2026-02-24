using Rtl.Core.Application.Adapters.ISeries;
using Rtl.Core.Application.Adapters.ISeries.Pricing;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain.Results;
using Modules.Sales.Domain.Sales;

namespace Modules.Sales.Application.Pricing.GetWheelsAndAxlesPriceByStock;

// Flow: GET /api/v1/sales/{saleId}/pricing/wheels-and-axles-by-stock?stockNumber=X&isRetaining=false →
//   resolve sale → iSeries by-stock-number calculation → return W&A price
internal sealed class GetWheelsAndAxlesPriceByStockQueryHandler(
    ISaleRepository saleRepository,
    IiSeriesAdapter iSeriesAdapter)
    : IQueryHandler<GetWheelsAndAxlesPriceByStockQuery, decimal>
{
    public async Task<Result<decimal>> Handle(
        GetWheelsAndAxlesPriceByStockQuery request,
        CancellationToken cancellationToken)
    {
        var sale = await saleRepository.GetByPublicIdAsync(
            request.PublicSaleId, cancellationToken);

        if (sale is null)
        {
            return Result.Failure<decimal>(
                SaleErrors.NotFoundByPublicId(request.PublicSaleId));
        }

        var homeCenterNumber = sale.RetailLocation.RefHomeCenterNumber ?? 0;

        var adapterRequest = new WheelAndAxlePriceByStockRequest
        {
            HomeCenterNumber = homeCenterNumber,
            StockNumber = request.StockNumber,
            IsRetaining = request.IsRetaining
        };

        var price = await iSeriesAdapter.GetWheelAndAxlePriceByStock(adapterRequest, cancellationToken);

        return price;
    }
}
