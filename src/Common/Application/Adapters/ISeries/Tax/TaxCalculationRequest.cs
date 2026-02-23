namespace Rtl.Core.Application.Adapters.ISeries.Tax;

public sealed class TaxCalculationRequest
{
    public int HomeCenterNumber { get; init; }
    public int AppId { get; init; }
    public string StockNumber { get; init; } = string.Empty;
    public ModularClassification ModularClassification { get; init; }
    public decimal WarrantyAmount { get; init; }
    public HomeCondition HomeCondition { get; init; }
    public int NumberOfFloorSections { get; init; }
}
