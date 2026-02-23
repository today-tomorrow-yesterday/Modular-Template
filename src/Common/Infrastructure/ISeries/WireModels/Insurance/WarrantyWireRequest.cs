namespace Rtl.Core.Infrastructure.ISeries.WireModels.Insurance;

// Property names MUST match the iSeries gateway's GetHomeBuyersProtectionPlanQuoteParameters DTO.
// Gateway expects raw WidthInFeet/ModelYear — it computes HomeAge/HomeFloors internally.
internal sealed class WarrantyWireRequest
{
    public int MasterDealerNumber { get; set; } = 29;
    public int HomeCenterNumber { get; set; }
    public int AppId { get; set; }
    public string PhysicalState { get; set; } = string.Empty;
    public string PhysicalZip { get; set; } = string.Empty;
    public int WidthInFeet { get; set; }
    public int ModelYear { get; set; }
    public string HomeType { get; set; } = string.Empty;
    public string HudOrMod { get; set; } = string.Empty;
    public bool IsInCityLimits { get; set; }
    public bool CalculateSalesTax { get; set; }
}

internal sealed class WarrantyWireResponse
{
    public decimal Premium { get; set; }
    public decimal SalesTaxPremium { get; set; }
}
