using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.Packages.UpdatePackageLand;

public sealed record UpdatePackageLandCommand(
    Guid PackagePublicId,
    decimal SalePrice,
    decimal EstimatedCost,
    decimal RetailSalePrice,
    string LandPurchaseType,
    string? TypeOfLandWanted,
    string? CustomerLandType,
    string? LandInclusion,
    string? LandStockNumber,
    decimal? LandSalesPrice,
    decimal? LandCost,
    string? PropertyOwner,
    string? FinancedBy,
    decimal? EstimatedValue,
    decimal? SizeInAcres,
    decimal? PayoffAmountFinancing,
    decimal? LandEquity,
    DateTime? OriginalPurchaseDate,
    decimal? OriginalPurchasePrice,
    string? Realtor,
    decimal? PurchasePrice,
    string? PropertyOwnerPhoneNumber,
    decimal? PropertyLotRent,
    int? CommunityNumber,
    string? CommunityName,
    string? CommunityManagerName,
    string? CommunityManagerPhoneNumber,
    string? CommunityManagerEmail,
    decimal? CommunityMonthlyCost) : ICommand<UpdatePackageLandResult>;

public sealed record UpdatePackageLandResult(
    decimal GrossProfit,
    decimal CommissionableGrossProfit,
    bool MustRecalculateTaxes);
