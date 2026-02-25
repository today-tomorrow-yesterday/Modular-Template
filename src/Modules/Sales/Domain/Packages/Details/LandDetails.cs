using Rtl.Core.Domain.Entities;
using System.Text.Json;

namespace Modules.Sales.Domain.Packages.Details;

public enum LandPurchaseType
{
    CustomerHasLand,
    CustomerWantsToPurchaseLand
}

public enum CustomerLandType
{
    CustomerOwnedLand,
    PrivateProperty,
    CommunityOrNeighborhood
}

public enum LandInclusion
{
    CustomerLandPayoff,
    CustomerLandInLieu,
    HomeOnly
}

public enum TypeOfLandWanted
{
    LandPurchase,
    HomeCenterOwnedLand
}

// Conditional fields depend on LandPurchaseType and CustomerLandType combinations.
public sealed class LandDetails : IVersionedDetails
{
    public int SchemaVersion => 1;
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }

    public LandPurchaseType LandPurchaseType { get; private set; }
    public CustomerLandType? CustomerLandType { get; private set; }
    public LandInclusion? LandInclusion { get; private set; }

    public decimal? EstimatedValue { get; private set; }
    public decimal? SizeInAcres { get; private set; }
    public TypeOfLandWanted? TypeOfLandWanted { get; private set; }

    // CustomerOwnedLand
    public string? PropertyOwner { get; private set; }
    public string? FinancedBy { get; private set; }

    // CustomerLandPayoff
    public decimal? PayoffAmountFinancing { get; private set; }
    public decimal? LandEquity { get; private set; }
    public DateTimeOffset? OriginalPurchaseDate { get; private set; }
    public decimal? OriginalPurchasePrice { get; private set; }

    // PrivateProperty
    public string? PropertyOwnerPhoneNumber { get; private set; }
    public decimal? PropertyLotRent { get; private set; }

    // CustomerWantsToPurchaseLand — LandPurchase
    public string? Realtor { get; private set; }
    public decimal? PurchasePrice { get; private set; }

    // CustomerWantsToPurchaseLand — HomeCenterOwnedLand
    public string? LandStockNumber { get; private set; }
    public decimal? LandCost { get; private set; }
    public decimal? LandSalesPrice { get; private set; }

    // CommunityOrNeighborhood
    public int? CommunityNumber { get; private set; }
    public string? CommunityName { get; private set; }
    public string? CommunityManagerName { get; private set; }
    public string? CommunityManagerPhoneNumber { get; private set; }
    public string? CommunityManagerEmail { get; private set; }
    public decimal? CommunityMonthlyCost { get; private set; }

    private LandDetails() { }

    public static LandDetails Create(
        LandPurchaseType landPurchaseType,
        CustomerLandType? customerLandType = null,
        LandInclusion? landInclusion = null,
        TypeOfLandWanted? typeOfLandWanted = null,
        decimal? estimatedValue = null,
        decimal? sizeInAcres = null,
        string? propertyOwner = null,
        string? financedBy = null,
        decimal? payoffAmountFinancing = null,
        decimal? landEquity = null,
        DateTimeOffset? originalPurchaseDate = null,
        decimal? originalPurchasePrice = null,
        string? propertyOwnerPhoneNumber = null,
        decimal? propertyLotRent = null,
        string? realtor = null,
        decimal? purchasePrice = null,
        string? landStockNumber = null,
        decimal? landCost = null,
        decimal? landSalesPrice = null,
        int? communityNumber = null,
        string? communityName = null,
        string? communityManagerName = null,
        string? communityManagerPhoneNumber = null,
        string? communityManagerEmail = null,
        decimal? communityMonthlyCost = null)
    {
        return new LandDetails
        {
            LandPurchaseType = landPurchaseType,
            CustomerLandType = customerLandType,
            LandInclusion = landInclusion,
            TypeOfLandWanted = typeOfLandWanted,
            EstimatedValue = estimatedValue,
            SizeInAcres = sizeInAcres,
            PropertyOwner = propertyOwner,
            FinancedBy = financedBy,
            PayoffAmountFinancing = payoffAmountFinancing,
            LandEquity = landEquity,
            OriginalPurchaseDate = originalPurchaseDate,
            OriginalPurchasePrice = originalPurchasePrice,
            PropertyOwnerPhoneNumber = propertyOwnerPhoneNumber,
            PropertyLotRent = propertyLotRent,
            Realtor = realtor,
            PurchasePrice = purchasePrice,
            LandStockNumber = landStockNumber,
            LandCost = landCost,
            LandSalesPrice = landSalesPrice,
            CommunityNumber = communityNumber,
            CommunityName = communityName,
            CommunityManagerName = communityManagerName,
            CommunityManagerPhoneNumber = communityManagerPhoneNumber,
            CommunityManagerEmail = communityManagerEmail,
            CommunityMonthlyCost = communityMonthlyCost
        };
    }
}
