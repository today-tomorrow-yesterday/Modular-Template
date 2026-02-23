namespace Rtl.Core.Application.Adapters.ISeries.Insurance;

public sealed class HomeFirstQuoteRequest
{
    public int HomeCenterNumber { get; init; }
    public string StockNumber { get; init; } = string.Empty;
    public string ModelNumber { get; init; } = string.Empty;
    public decimal CoverageAmount { get; init; }
    public int ModelYear { get; init; }
    public string DeliveryZipCode { get; init; } = string.Empty;
    public HomeCondition HomeCondition { get; init; }
    public string SerialNumber { get; init; } = string.Empty;
    public int LengthInFeet { get; init; }
    public int WidthInFeet { get; init; }
    public OccupancyType OccupancyType { get; init; }
    public bool InParkOrSubdivision { get; init; }
    public bool HasFoundationOrMasonry { get; init; }
    public bool IsLandOwnedByCustomer { get; init; }

    // Customer information
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string MailingAddress { get; init; } = string.Empty;
    public string MailingCity { get; init; } = string.Empty;
    public string MailingState { get; init; } = string.Empty;
    public string MailingZip { get; init; } = string.Empty;
    public string LocationAddress { get; init; } = string.Empty;
    public string LocationCity { get; init; } = string.Empty;
    public string LocationState { get; init; } = string.Empty;
    public bool IsWithinCityLimits { get; init; }
    public DateOnly? BuyerBirthDate { get; init; }
    public DateOnly? CoBuyerBirthDate { get; init; }
    public string? PhoneNumber { get; init; }
}
