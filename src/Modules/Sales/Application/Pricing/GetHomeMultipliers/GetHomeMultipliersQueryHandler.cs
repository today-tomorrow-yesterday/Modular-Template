using Modules.Sales.Domain.Cdc;
using Modules.Sales.Domain.Sales;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.Pricing.GetHomeMultipliers;

// Flow: GET /api/v1/sales/{saleId}/pricing/home-multipliers →
//   resolve sale → derive stateCode from RetailLocation → query CDC home multipliers
internal sealed class GetHomeMultipliersQueryHandler(
    ISaleRepository saleRepository,
    ICdcPricingQueries cdcPricingQueries)
    : IQueryHandler<GetHomeMultipliersQuery, HomeMultipliersResult>
{
    public async Task<Result<HomeMultipliersResult>> Handle(
        GetHomeMultipliersQuery request,
        CancellationToken cancellationToken)
    {
        var sale = await saleRepository.GetByPublicIdWithCustomerContextAsync(
            request.PublicSaleId, cancellationToken);

        if (sale is null)
        {
            return Result.Failure<HomeMultipliersResult>(
                SaleErrors.NotFoundByPublicId(request.PublicSaleId));
        }

        var stateCode = sale.RetailLocation.StateCode;

        var multiplier = await cdcPricingQueries.GetActiveMultiplierForStateAsync(
            stateCode, request.EffectiveDate, cancellationToken);

        if (multiplier is null)
        {
            return Result.Failure<HomeMultipliersResult>(Error.NotFound(
                "HomeMultipliers.NotFound",
                $"No active home multipliers found for state '{stateCode}'."));
        }

        return new HomeMultipliersResult(
            multiplier.EffectiveDate,
            multiplier.HomeMultiplierValue,
            multiplier.UpgradesMultiplier,
            multiplier.FreightMultiplier,
            multiplier.WheelsAxlesMultiplier,
            multiplier.DuesMultiplier);
    }
}
