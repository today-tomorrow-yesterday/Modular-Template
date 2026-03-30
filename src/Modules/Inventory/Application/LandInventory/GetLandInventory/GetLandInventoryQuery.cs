using Rtl.Core.Application.Messaging;

namespace Modules.Inventory.Application.LandInventory.GetLandInventory;

public sealed record GetLandInventoryQuery(int HomeCenterNumber) : IQuery<IReadOnlyCollection<LandInventoryResponse>>;

public sealed record LandInventoryResponse(
    Guid PublicId,
    int HomeCenterNumber,
    string StockNumber,
    string? StockType,
    string? LandAge,
    decimal? LandCost,
    decimal? AddToTotal,
    decimal? Appraisal,
    string? MapParcel,
    string? Address,
    string? Address2,
    string? City,
    string? State,
    string? Zip,
    string? County,
    string? LoanNumber,
    string? HomeStockNumber);
