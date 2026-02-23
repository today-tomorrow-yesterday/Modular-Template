using Rtl.Core.Domain.Entities;
using System.Text.Json;

namespace Modules.Sales.Domain.Packages.Details;

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
        int? tempLinkId = null)
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
            QuotedAt = DateTimeOffset.UtcNow
        };
    }
}
