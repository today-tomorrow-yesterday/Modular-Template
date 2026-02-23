using Rtl.Core.Application.EventBus;

namespace Modules.Inventory.IntegrationEvents;

// ECST — land cost or appraisal value changed. Consumers upsert cache
// and may trigger pricing review on affected downstream entities.
public sealed record LandParcelAppraisalRevisedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    int LandParcelId,
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
