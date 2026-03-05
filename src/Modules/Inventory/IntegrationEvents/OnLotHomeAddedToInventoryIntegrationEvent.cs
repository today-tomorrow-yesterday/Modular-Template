using Rtl.Core.Application.EventBus;

namespace Modules.Inventory.IntegrationEvents;

// ECST — first CDC sync for a new on-lot home. Consumers create their cache row.
[EventDetailType("rtl.inventory.onLotHomeAddedToInventory")]
public sealed record OnLotHomeAddedToInventoryIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    int OnLotHomeId,
    int HomeCenterNumber,
    string StockNumber,
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
    decimal? OriginalRetailPrice,
    decimal? CurrentRetailPrice) : IntegrationEvent(Id, OccurredOnUtc);
