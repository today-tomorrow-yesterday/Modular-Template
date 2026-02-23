using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.Tax.UpdatePackageTax;

public sealed record UpdatePackageTaxCommand(
    Guid PackagePublicId,
    string? PreviouslyTitled,
    int? TaxExemptionId,
    IReadOnlyCollection<TaxQuestionAnswerRequest> QuestionAnswers) : ICommand<UpdatePackageTaxResult>;

public sealed record TaxQuestionAnswerRequest(int QuestionNumber, bool Answer);

public sealed record UpdatePackageTaxResult(
    decimal GrossProfit,
    decimal CommissionableGrossProfit,
    bool MustRecalculateTaxes);
