using Rtl.Core.Application.Adapters.ISeries;
using Rtl.Core.Application.Adapters.ISeries.Pricing;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain.Results;
using Modules.Sales.Domain.Sales;

namespace Modules.Sales.Application.Pricing.GetWheelsAndAxlesPrice;

// Flow: GET /api/v1/sales/{saleId}/pricing/wheels-and-axles →
//   resolve sale → iSeries by-count calculation → return W&A price
internal sealed class GetWheelsAndAxlesPriceQueryHandler(
    ISaleRepository saleRepository,
    IiSeriesAdapter iSeriesAdapter)
    : IQueryHandler<GetWheelsAndAxlesPriceQuery, decimal>
{
    public async Task<Result<decimal>> Handle(
        GetWheelsAndAxlesPriceQuery request,
        CancellationToken cancellationToken)
    {
        var sale = await saleRepository.GetByPublicIdAsync(
            request.PublicSaleId, cancellationToken);

        if (sale is null)
        {
            return Result.Failure<decimal>(
                SaleErrors.NotFoundByPublicId(request.PublicSaleId));
        }

        var adapterRequest = new WheelAndAxlePriceByCountRequest
        {
            NumberOfWheels = request.NumberOfWheels,
            NumberOfAxles = request.NumberOfAxles
        };

        var price = await iSeriesAdapter.CalculateWheelAndAxlePrice(adapterRequest, cancellationToken);

        return price;
    }
}
