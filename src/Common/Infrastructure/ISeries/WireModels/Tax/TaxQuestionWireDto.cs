namespace Rtl.Core.Infrastructure.ISeries.WireModels.Tax;

internal sealed class TaxQuestionWireDto
{
    public int AppId { get; set; }
    public int CustomerNumber { get; set; }
    public int QuestionNumber { get; set; }
    public bool QuestionAnswer { get; set; }
}
