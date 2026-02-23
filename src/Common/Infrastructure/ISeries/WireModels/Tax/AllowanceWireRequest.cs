namespace Rtl.Core.Infrastructure.ISeries.WireModels.Tax;

// Property names MUST match the iSeries gateway's UpsertAllowanceInformationParameters DTO.
internal sealed class AllowanceWireRequest
{
    public int MasterDealerNumber { get; set; } = 29;
    public int CustomerNumber { get; set; } = 0;
    public string PointOfDeliveryDomicileCode { get; set; } = string.Empty;

    public int AppId { get; set; }
    public Guid CorrelationId { get; set; }
    public int LotNumber { get; set; }

    // Home
    public decimal HomeSalePrice { get; set; }
    public decimal HomeNetInvoice { get; set; }
    public int NumberOfFloors { get; set; }
    public decimal FreightCost { get; set; }
    public decimal CarrierFrameDeposit { get; set; }
    public decimal HomeGrossInvoiceCost { get; set; }
    public decimal HomeTaxOnInvoice { get; set; }
    public decimal ManufacturerRebate { get; set; }
    public char TransportType { get; set; }

    // Trade-in
    public decimal TradeInAllowance { get; set; }
    public decimal TradeInOverAllowance { get; set; }
    public char TradeInType { get; set; } = ' ';

    // Tax config
    public string PreviouslyTitledInState { get; set; } = string.Empty;
    public bool IsTaxExempt { get; set; }

    // Delivery address — gateway expects "PointOfDelivery" prefix
    public string PointOfDeliveryCity { get; set; } = string.Empty;
    public string PointOfDeliveryCounty { get; set; } = string.Empty;
    public string PointOfDeliveryZipCode { get; set; } = string.Empty;
    public bool IsPointOfDeliveryInCityLimits { get; set; }

    // Point of sale
    public string PointOfSaleZipCode { get; set; } = string.Empty;

    // Add-ons — gateway expects "AddOnOptions"
    public decimal TotalAddOnCost { get; set; }
    public decimal TotalAddOnSalePrice { get; set; }
    public AllowanceAddOnWire[] AddOnOptions { get; set; } = [];
}

// Property names MUST match the iSeries gateway's AddOnOptionParameter DTO.
internal sealed class AllowanceAddOnWire
{
    public int CategoryId { get; set; }
    public int ItemId { get; set; }
    public decimal Cost { get; set; }
    public decimal SalePrice { get; set; }
}
