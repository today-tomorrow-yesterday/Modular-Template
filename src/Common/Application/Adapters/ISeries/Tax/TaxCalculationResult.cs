namespace Rtl.Core.Application.Adapters.ISeries.Tax;

public sealed class TaxCalculationResult
{
    public decimal StateTax { get; init; }
    public decimal CityTax { get; init; }
    public decimal CountyTax { get; init; }
    public decimal Basis { get; init; }
    public decimal UseTax { get; init; }
    public decimal? GrossReceiptCityTax { get; init; }
    public decimal? GrossReceiptCountyTax { get; init; }
    public decimal? ManufacturedHomeInventoryTax { get; init; }
    public List<string>? Messages { get; init; }
}
