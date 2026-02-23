namespace Rtl.Core.Application.Adapters.ISeries.Insurance;

public sealed class WarrantyQuoteRequest
{
    public int HomeCenterNumber { get; init; }
    public int AppId { get; init; }
    public string PhysicalState { get; init; } = string.Empty;
    public string PhysicalZip { get; init; } = string.Empty;
    public int WidthInFeet { get; init; }
    public int ModelYear { get; init; }
    public HomeCondition HomeCondition { get; init; }
    public ModularClassification ModularClassification { get; init; }
    public bool IsWithinCityLimits { get; init; }
    public bool CalculateSalesTax { get; init; }
}
