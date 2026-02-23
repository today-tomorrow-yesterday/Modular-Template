using Rtl.Core.Application.Adapters.ISeries;
using Rtl.Core.Application.Adapters.ISeries.Pricing;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain.Results;
using Modules.Sales.Domain.Sales;

namespace Modules.Sales.Application.Pricing.GetOptionTotals;

// Flow: GET /api/v1/sales/{saleId}/pricing/option-totals →
//   resolve sale → derive stateCode → iSeries POST /v1/pricing/option-totals → return totals
internal sealed class GetOptionTotalsQueryHandler(
    ISaleRepository saleRepository,
    IiSeriesAdapter iSeriesAdapter)
    : IQueryHandler<GetOptionTotalsQuery, OptionTotalsResult>
{
    public async Task<Result<OptionTotalsResult>> Handle(
        GetOptionTotalsQuery request,
        CancellationToken cancellationToken)
    {
        var sale = await saleRepository.GetByPublicIdWithPartyContextAsync(
            request.PublicSaleId, cancellationToken);

        if (sale is null)
        {
            return Result.Failure<OptionTotalsResult>(
                SaleErrors.NotFoundByPublicId(request.PublicSaleId));
        }

        var effectiveDate = DateOnly.TryParse(request.EffectiveDate, out var parsed)
            ? parsed
            : DateOnly.FromDateTime(DateTime.UtcNow);

        var adapterRequest = new OptionTotalsRequest
        {
            HomeCenterState = sale.RetailLocation.StateCode,
            EffectiveDate = effectiveDate,
            PlantNumber = request.PlantNumber,
            QuoteNumber = request.QuoteNumber,
            OrderNumber = request.OrderNumber
        };

        var result = await iSeriesAdapter.CalculateOptionTotals(adapterRequest, cancellationToken);

        return new OptionTotalsResult(result.FactoryOptionTotal, result.RetailOptionTotal);
    }
}
