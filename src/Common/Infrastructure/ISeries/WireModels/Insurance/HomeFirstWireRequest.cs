namespace Rtl.Core.Infrastructure.ISeries.WireModels.Insurance;

// Property names MUST match the iSeries gateway's GetHomeFirstQuoteParameters DTO
// (gateway deserializes by property name via System.Text.Json with camelCase policy).
internal sealed class HomeFirstWireRequest
{
    public int LotNumber { get; set; }
    public string HomeStockNumber { get; set; } = string.Empty;
    public string HomeModel { get; set; } = string.Empty;
    public decimal CoverageAmount { get; set; }
    public int HomeYearBuilt { get; set; }
    public string HomeType { get; set; } = string.Empty;
    public string HomeSerial { get; set; } = string.Empty;
    public int HomeLength { get; set; }
    public int HomeWidth { get; set; }
    public char OccupancyType { get; set; }
    public bool IsHomeLocatedInPark { get; set; }
    public bool IsHomeOnPermanentFoundation { get; set; }
    public bool IsLandCustomerOwned { get; set; }
    public bool IsInCityLimits { get; set; }

    // Customer
    public string CustomerFirstName { get; set; } = string.Empty;
    public string CustomerLastName { get; set; } = string.Empty;
    public string MailingAddress { get; set; } = string.Empty;
    public string MailingCity { get; set; } = string.Empty;
    public string MailingState { get; set; } = string.Empty;
    public string MailingZip { get; set; } = string.Empty;
    public string PhysicalAddress { get; set; } = string.Empty;
    public string PhysicalCity { get; set; } = string.Empty;
    public string PhysicalState { get; set; } = string.Empty;
    public string PhysicalZip { get; set; } = string.Empty;
    public DateTime? CustomerBirthDate { get; set; }
    public DateTime? CoApplicantBirthDate { get; set; }
    public string? HomePhone { get; set; }
}

internal sealed class HomeFirstWireResponse
{
    public string InsuranceCompanyName { get; set; } = string.Empty;
    public decimal TotalPremium { get; set; }
    public decimal MaximumCoverage { get; set; }
    public int TempLinkId { get; set; }
    public string? ErrorMessage { get; set; }
}
