using Rtl.Core.Application.Messaging;

namespace Modules.Inventory.Application.OnLotInventory.GetOnLotInventory;

public sealed record GetOnLotInventoryQuery(int HomeCenterNumber) : IQuery<IReadOnlyCollection<OnLotInventoryResponse>>;

public sealed record OnLotInventoryResponse(
    Guid PublicId,
    int HomeCenterNumber,
    string StockNumber,
    string? StockType,
    string? Condition,
    string? BuildType,
    decimal? Width,
    decimal? Length,
    int? Bedrooms,
    int? Bathrooms,
    int? ModelYear,
    decimal? TotalInvoiceAmount,
    decimal? PurchaseDiscount,
    decimal? OriginalRetailPrice,
    decimal? CurrentRetailPrice,
    string? Model,
    string? Make,
    string? Facility,
    string? SerialNumber,
    string? StockedInDate,
    string? LandStockNumber,
    OnLotLandCostsResponse? LandCosts,
    OnLotAncillaryDataResponse? AncillaryData);

public sealed record OnLotLandCostsResponse(
    decimal? AddToTotal,
    decimal? FurnitureTotal);

public sealed record OnLotAncillaryDataResponse(
    string? CustomerName,
    DateTime? PackageReceivedDate);
