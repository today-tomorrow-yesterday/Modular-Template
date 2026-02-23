using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.Tax.GetTaxExemptions;

public sealed record GetTaxExemptionsQuery : IQuery<IReadOnlyCollection<TaxExemptionResult>>;

public sealed record TaxExemptionResult(
    int ExemptionCode,
    string? Description,
    string? RulesText);
