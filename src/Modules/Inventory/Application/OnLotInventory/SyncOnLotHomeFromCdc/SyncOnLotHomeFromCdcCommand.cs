using Rtl.Core.Application.Messaging;

namespace Modules.Inventory.Application.OnLotInventory.SyncOnLotHomeFromCdc;

public sealed record SyncOnLotHomeFromCdcCommand(
    int OnLotHomeId,
    int RefHomeCenterNumber,
    string RefStockNumber,
    string? StockType,
    string? Condition,
    string? BuildType,
    decimal? Width,
    decimal? Length,
    int? NumberOfBedrooms,
    int? NumberOfBathrooms,
    int? ModelYear,
    string? Model,
    string? Make,
    string? Facility,
    string? SerialNumber,
    decimal? TotalInvoiceAmount,
    decimal? PurchaseDiscount,
    decimal? OriginalRetailPrice,
    decimal? CurrentRetailPrice,
    string? StockedInDate,
    string? LandStockNumber) : ICommand;
