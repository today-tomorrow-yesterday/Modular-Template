using Rtl.Core.Application.EventBus;

namespace Modules.Inventory.IntegrationEvents;

// ECST — non-cost, non-availability fields changed (address, county, stock type,
// etc.). Catch-all for property changes not covered by the specific
// AppraisalRevised or AvailabilityChanged events.
[EventDetailType("rtl.inventory.landParcelDetailsRevised")]
public sealed record LandParcelDetailsRevisedIntegrationEvent(
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
