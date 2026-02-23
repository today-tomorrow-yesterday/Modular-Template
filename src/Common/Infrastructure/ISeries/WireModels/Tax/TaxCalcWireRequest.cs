namespace Rtl.Core.Infrastructure.ISeries.WireModels.Tax;

internal sealed class TaxCalcWireRequest
{
    public int MasterDealerNumber { get; set; } = 29;
    public int VersionNumber { get; set; } = 0;
    public int LotNumber { get; set; }
    public int AppId { get; set; }
    public int CustomerNumber { get; set; } = 0;
    public string StockNumber { get; set; } = string.Empty;
    public string DomicileCode { get; set; } = string.Empty;
    public char ModCode { get; set; }
    public decimal Hbpp { get; set; }
}

internal sealed class TaxCalcWireResponse
{
    public decimal StateTax { get; set; }
    public decimal CityTax { get; set; }
    public decimal CountyTax { get; set; }
    public decimal Basis { get; set; }
    public decimal UseTax { get; set; }
    public decimal? GrossReceiptCityTax { get; set; }
    public decimal? GrossReceiptCountyTax { get; set; }
    public decimal? ManufacturedHomeInventoryTax { get; set; }
    public List<string>? Messages { get; set; }
}
