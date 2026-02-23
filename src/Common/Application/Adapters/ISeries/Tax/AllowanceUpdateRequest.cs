namespace Rtl.Core.Application.Adapters.ISeries.Tax;

public sealed class AllowanceUpdateRequest
{
    public int AppId { get; init; }
    public Guid CorrelationId { get; init; }
    public int HomeCenterNumber { get; init; }

    // Home
    public decimal HomeSalePrice { get; init; }
    public decimal HomeNetInvoice { get; init; }
    public int NumberOfFloorSections { get; init; }
    public decimal FreightCost { get; init; }
    public decimal CarrierFrameDeposit { get; init; }
    public decimal GrossCost { get; init; }
    public decimal TaxIncludedOnInvoice { get; init; }
    public decimal RebateOnMfgInvoice { get; init; }
    public HomeCondition HomeCondition { get; init; }

    // Trade-in
    public decimal TradeAllowance { get; init; }
    public decimal BookInAmount { get; init; }
    public char? TradeInType { get; init; }

    // Tax config
    public string PreviouslyTitled { get; init; } = string.Empty;
    public bool IsTaxExempt { get; init; }

    // Delivery address
    public string City { get; init; } = string.Empty;
    public string County { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public bool IsWithinCityLimits { get; init; }

    // Point of sale
    public string PointOfSaleZip { get; init; } = string.Empty;

    // Add-ons
    public decimal TotalAddOnCost { get; init; }
    public decimal TotalAddOnSalePrice { get; init; }
    public AllowanceAddOn[] AddOns { get; init; } = [];
}

public sealed class AllowanceAddOn
{
    public int CategoryNumber { get; init; }
    public int ItemNumber { get; init; }
    public decimal Cost { get; init; }
    public decimal SalePrice { get; init; }
}
