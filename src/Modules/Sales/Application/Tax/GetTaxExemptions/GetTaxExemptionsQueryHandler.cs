using Modules.Sales.Domain.Cdc;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.Tax.GetTaxExemptions;

internal sealed class GetTaxExemptionsQueryHandler(ICdcTaxQueries cdcTaxQueries)
    : IQueryHandler<GetTaxExemptionsQuery, IReadOnlyCollection<TaxExemptionResult>>
{
    public async Task<Result<IReadOnlyCollection<TaxExemptionResult>>> Handle(
        GetTaxExemptionsQuery request,
        CancellationToken cancellationToken)
    {
        var exemptions = await cdcTaxQueries.GetActiveExemptionsAsync(cancellationToken);

        var results = exemptions.Select(e => new TaxExemptionResult(
            e.ExemptionCode,
            e.Description,
            e.RulesText)).ToList();

        return results;
    }
}
