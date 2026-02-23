namespace Rtl.Core.Application.Adapters.ISeries.Pricing;

public sealed class RetailPriceRequest
{
    public string HomeCenterState { get; init; } = string.Empty;
    public DateOnly EffectiveDate { get; init; }
    public string SerialNumber { get; init; } = string.Empty;
    public decimal InvoiceTotalAmount { get; init; }
    public int NumberOfAxles { get; init; }
    public decimal FactoryOptionTotal { get; init; }
    public decimal RetailOptionTotal { get; init; }
    public string ModelNumber { get; init; } = string.Empty;
    public decimal? BaseCost { get; init; }
}
