using Rtl.Core.Application.Messaging;

namespace Modules.Inventory.Application.LandInventory.SyncLandParcelFromCdc;

public sealed record SyncLandParcelFromCdcCommand(
    int LandParcelId,
    int RefHomeCenterNumber,
    string RefStockNumber,
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
    string? HomeStockNumber) : ICommand;
