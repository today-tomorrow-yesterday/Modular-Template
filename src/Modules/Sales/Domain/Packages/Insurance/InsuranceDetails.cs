using Rtl.Core.Domain.Entities;
using System.Text.Json;

namespace Modules.Sales.Domain.Packages.Insurance;

public enum InsuranceType
{
    HomeFirst,
    Warranty,
    Outside
}

public sealed class InsuranceDetails : IVersionedDetails
{
    public int SchemaVersion => 1;
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }

    public InsuranceType InsuranceType { get; private set; }
    public decimal CoverageAmount { get; private set; }
    public bool HasFoundationOrMasonry { get; private set; }
    public bool InParkOrSubdivision { get; private set; }
    public bool IsLandOwnedByCustomer { get; private set; }
    public bool IsPremiumFinanced { get; private set; }
    public int? QuoteId { get; private set; }
    public string? CompanyName { get; private set; }
    public decimal? MaxCoverage { get; private set; }
    public decimal? TotalPremium { get; private set; }
    public string? ProviderName { get; private set; } // Outside: user-entered; HomeFirst: iSeries-returned
    public int? TempLinkId { get; private set; } // HomeFirst: TempLinkId (1-10000) for PDF retrieval
    public DateTimeOffset? QuotedAt { get; private set; } // When the quote was generated

    // Home context at quote time
    public string? HomeStockNumber { get; private set; }
    public int? HomeModelYear { get; private set; }
    public decimal? HomeLengthInFeet { get; private set; }
    public decimal? HomeWidthInFeet { get; private set; }
    public string? HomeCondition { get; private set; } // New/Used/Repo

    // Location context at quote time
    public string? DeliveryState { get; private set; }
    public string? DeliveryPostalCode { get; private set; }
    public string? DeliveryCity { get; private set; }
    public bool? DeliveryIsWithinCityLimits { get; private set; }
    public string? OccupancyType { get; private set; } // Primary/Secondary/Rental — eligibility critical

    private InsuranceDetails() { }

    public static InsuranceDetails Create(
        InsuranceType insuranceType,
        decimal coverageAmount,
        bool hasFoundationOrMasonry = false,
        bool inParkOrSubdivision = false,
        bool isLandOwnedByCustomer = false,
        bool isPremiumFinanced = false,
        int? quoteId = null,
        string? companyName = null,
        decimal? maxCoverage = null,
        decimal? totalPremium = null,
        string? providerName = null,
        int? tempLinkId = null,
        DateTimeOffset? quotedAt = null,
        string? homeStockNumber = null,
        int? homeModelYear = null,
        decimal? homeLengthInFeet = null,
        decimal? homeWidthInFeet = null,
        string? homeCondition = null,
        string? deliveryState = null,
        string? deliveryPostalCode = null,
        string? deliveryCity = null,
        bool? deliveryIsWithinCityLimits = null,
        string? occupancyType = null)
    {
        return new InsuranceDetails
        {
            InsuranceType = insuranceType,
            CoverageAmount = coverageAmount,
            HasFoundationOrMasonry = hasFoundationOrMasonry,
            InParkOrSubdivision = inParkOrSubdivision,
            IsLandOwnedByCustomer = isLandOwnedByCustomer,
            IsPremiumFinanced = isPremiumFinanced,
            QuoteId = quoteId,
            CompanyName = companyName,
            MaxCoverage = maxCoverage,
            TotalPremium = totalPremium,
            ProviderName = providerName,
            TempLinkId = tempLinkId,
            QuotedAt = quotedAt ?? DateTimeOffset.UtcNow,
            HomeStockNumber = homeStockNumber,
            HomeModelYear = homeModelYear,
            HomeLengthInFeet = homeLengthInFeet,
            HomeWidthInFeet = homeWidthInFeet,
            HomeCondition = homeCondition,
            DeliveryState = deliveryState,
            DeliveryPostalCode = deliveryPostalCode,
            DeliveryCity = deliveryCity,
            DeliveryIsWithinCityLimits = deliveryIsWithinCityLimits,
            OccupancyType = occupancyType
        };
    }
}
