namespace Rtl.Core.Infrastructure.ISeries.WireModels.Pricing;

internal sealed class OptionTotalsWireRequest
{
    public string HomeCenterState { get; set; } = string.Empty;
    public string EffectiveDate { get; set; } = string.Empty;
    public decimal PlantNumber { get; set; }
    public decimal QuoteNumber { get; set; }
    public decimal OrderNumber { get; set; }
}

internal sealed class OptionTotalsWireResponse
{
    public decimal HbgOptionTotal { get; set; }
    public decimal RetailOptionTotal { get; set; }
}
