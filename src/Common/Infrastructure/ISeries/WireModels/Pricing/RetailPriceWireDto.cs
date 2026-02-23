namespace Rtl.Core.Infrastructure.ISeries.WireModels.Pricing;

internal sealed class RetailPriceWireRequest
{
    public string HomeCenterState { get; set; } = string.Empty;
    public string EffectiveDate { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public decimal InvoiceTotalAmount { get; set; }
    public int NumberOfAxles { get; set; }
    public decimal HbgOptionTotal { get; set; }
    public decimal RetailOptionTotal { get; set; }
    public string ModelNumber { get; set; } = string.Empty;
    public decimal? BaseCost { get; set; }
}

internal sealed class RetailPriceWireResponse
{
    public decimal RetailPrice { get; set; }
}

internal sealed class InventoryAncillaryWireResponse
{
    public decimal WheelAndAxlePrice { get; set; }
}
