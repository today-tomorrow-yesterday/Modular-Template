using Modules.Sales.Domain.Sales;
using Rtl.Core.Application.Adapters.ISeries;
using Rtl.Core.Application.Adapters.ISeries.Pricing;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.Pricing.GetRetailPrice;

// Flow: GET /api/v1/sales/{saleId}/pricing/retail-price →
//   resolve sale → derive stateCode → iSeries POST /v1/pricing/retail → return retail price
internal sealed class GetRetailPriceQueryHandler(
    ISaleRepository saleRepository,
    IiSeriesAdapter iSeriesAdapter)
    : IQueryHandler<GetRetailPriceQuery, decimal>
{
    public async Task<Result<decimal>> Handle(
        GetRetailPriceQuery request,
        CancellationToken cancellationToken)
    {
        var sale = await saleRepository.GetByPublicIdWithPartyContextAsync(
            request.PublicSaleId, cancellationToken);

        if (sale is null)
        {
            return Result.Failure<decimal>(
                SaleErrors.NotFoundByPublicId(request.PublicSaleId));
        }

        var effectiveDate = DateOnly.TryParse(request.EffectiveDate, out var parsed)
            ? parsed
            : DateOnly.FromDateTime(DateTime.UtcNow);

        var adapterRequest = new RetailPriceRequest
        {
            HomeCenterState = sale.RetailLocation.StateCode,
            EffectiveDate = effectiveDate,
            SerialNumber = request.SerialNumber,
            InvoiceTotalAmount = request.InvoiceTotal,
            NumberOfAxles = request.NumberOfAxles,
            FactoryOptionTotal = request.HbgOptionTotal,
            RetailOptionTotal = request.RetailOptionTotal,
            ModelNumber = request.ModelNumber,
            BaseCost = request.BaseCost
        };

        var retailPrice = await iSeriesAdapter.CalculateRetailPrice(adapterRequest, cancellationToken);

        return retailPrice;
    }
}
