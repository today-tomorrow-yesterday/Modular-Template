using Rtl.Core.Application.EventBus;

namespace Modules.Inventory.IntegrationEvents;

// Process Trigger — home removed from iSeries CDC feed (deleted/deactivated).
// Lean payload: just identity. Consumers remove their cache and detach package lines.
[EventDetailType("rtl.inventory.onLotHomeRemovedFromInventory")]
public sealed record OnLotHomeRemovedFromInventoryIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid PublicOnLotHomeId) : IntegrationEvent(Id, OccurredOnUtc);
