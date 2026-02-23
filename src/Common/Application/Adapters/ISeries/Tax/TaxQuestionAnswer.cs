namespace Rtl.Core.Application.Adapters.ISeries.Tax;

public sealed class TaxQuestionAnswer
{
    public int AppId { get; init; }
    public int CustomerNumber { get; init; }
    public int QuestionNumber { get; init; }
    public bool Answer { get; init; }
}
