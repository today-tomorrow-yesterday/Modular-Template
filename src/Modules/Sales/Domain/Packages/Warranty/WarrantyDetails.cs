using Rtl.Core.Domain.Entities;
using System.Text.Json;

namespace Modules.Sales.Domain.Packages.Warranty;

public sealed class WarrantyDetails : IVersionedDetails
{
    private WarrantyDetails() { }

    public int SchemaVersion => 1;
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }

    public bool WarrantySelected { get; private set; } // Whether warranty is selected — tax-affecting field
    public decimal? WarrantyAmount { get; private set; } // Warranty premium amount — tax-affecting field
    public decimal? SalesTaxPremium { get; private set; } // Sales tax on the warranty premium
    public DateTimeOffset? QuotedAt { get; private set; } // When the quote was generated

    // Home context at quote time
    public int? HomeModelYear { get; private set; }
    public string? HomeModularType { get; private set; } // Enum stored as string for snapshot stability
    public decimal? HomeWidthInFeet { get; private set; }
    public string? HomeCondition { get; private set; } // New/Used/Repo — derived from HomeType

    // Location context at quote time
    public string? DeliveryState { get; private set; }
    public string? DeliveryPostalCode { get; private set; }
    public bool? DeliveryIsWithinCityLimits { get; private set; }

    // Retail context
    public int? HomeCenterNumber { get; private set; }

    public static WarrantyDetails Create(
        decimal warrantyAmount,
        decimal salesTaxPremium,
        bool warrantySelected = true,
        DateTimeOffset? quotedAt = null,
        int? homeModelYear = null,
        string? homeModularType = null,
        decimal? homeWidthInFeet = null,
        string? homeCondition = null,
        string? deliveryState = null,
        string? deliveryPostalCode = null,
        bool? deliveryIsWithinCityLimits = null,
        int? homeCenterNumber = null)
    {
        return new WarrantyDetails
        {
            WarrantySelected = warrantySelected,
            WarrantyAmount = warrantyAmount,
            SalesTaxPremium = salesTaxPremium,
            QuotedAt = quotedAt ?? DateTimeOffset.UtcNow,
            HomeModelYear = homeModelYear,
            HomeModularType = homeModularType,
            HomeWidthInFeet = homeWidthInFeet,
            HomeCondition = homeCondition,
            DeliveryState = deliveryState,
            DeliveryPostalCode = deliveryPostalCode,
            DeliveryIsWithinCityLimits = deliveryIsWithinCityLimits,
            HomeCenterNumber = homeCenterNumber
        };
    }
}
