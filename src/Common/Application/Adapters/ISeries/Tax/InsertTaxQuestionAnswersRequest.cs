namespace Rtl.Core.Application.Adapters.ISeries.Tax;

public sealed class InsertTaxQuestionAnswersRequest
{
    public List<TaxQuestionAnswer> Answers { get; init; } = [];
}
