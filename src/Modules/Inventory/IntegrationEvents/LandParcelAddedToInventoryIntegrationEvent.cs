using Rtl.Core.Application.EventBus;

namespace Modules.Inventory.IntegrationEvents;

// ECST — first CDC sync for a new land parcel. Consumers create their cache row.
[EventDetailType("rtl.inventory.landParcelAddedToInventory")]
public sealed record LandParcelAddedToInventoryIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid PublicLandParcelId,
    int HomeCenterNumber,
    string StockNumber,
    string? StockType,
    decimal? LandCost,
    decimal? Appraisal,
    string? Address,
    string? City,
    string? State,
    string? Zip,
    string? County) : IntegrationEvent(Id, OccurredOnUtc);
