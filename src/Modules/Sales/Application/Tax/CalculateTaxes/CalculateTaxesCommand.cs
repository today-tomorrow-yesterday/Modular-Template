using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.Tax.CalculateTaxes;

// Journey B: POST /packages/{packageId}/tax — empty request body.
// All tax config (PreviouslyTitled, TaxExemptionId, QuestionAnswers) is read from the existing Tax line.
public sealed record CalculateTaxesCommand(
    Guid PackagePublicId) : ICommand<CalculateTaxesResult>;

public sealed record CalculateTaxesResult(
    decimal GrossProfit,
    decimal CommissionableGrossProfit,
    bool MustRecalculateTaxes,
    decimal TaxSalePrice,
    IReadOnlyCollection<TaxItemResult> TaxItems,
    IReadOnlyCollection<string> Errors);

public sealed record TaxItemResult(
    string Name,
    bool IsOverridden,
    decimal? CalculatedAmount,
    decimal? ChargedAmount);
