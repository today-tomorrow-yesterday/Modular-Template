namespace Rtl.Core.Application.Adapters.ISeries.Pricing;

public sealed class WheelAndAxlePriceByStockRequest
{
    public int HomeCenterNumber { get; init; }
    public string StockNumber { get; init; } = string.Empty;
    public bool IsRetaining { get; init; } // Determines rent (Cat 1/Item 28) vs purchase (Cat 1/Item 29)
}
