namespace Rtl.Core.Application.Adapters.ISeries.Pricing;

public sealed class OptionTotalsRequest
{
    public string HomeCenterState { get; init; } = string.Empty;
    public DateOnly EffectiveDate { get; init; }
    public decimal PlantNumber { get; init; }
    public decimal QuoteNumber { get; init; }
    public decimal OrderNumber { get; init; }
}
