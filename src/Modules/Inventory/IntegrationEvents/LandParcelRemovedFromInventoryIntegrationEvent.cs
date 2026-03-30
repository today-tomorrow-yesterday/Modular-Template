using Rtl.Core.Application.EventBus;

namespace Modules.Inventory.IntegrationEvents;

// Process Trigger — land parcel removed from iSeries CDC feed (deleted/deactivated).
// Lean payload: just identity. Consumers remove their cache and detach package lines.
[EventDetailType("rtl.inventory.landParcelRemovedFromInventory")]
public sealed record LandParcelRemovedFromInventoryIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid PublicLandParcelId) : IntegrationEvent(Id, OccurredOnUtc);
