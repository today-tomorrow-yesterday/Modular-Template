using Rtl.Core.Application.EventBus;

namespace Modules.Customer.IntegrationEvents;

// Published when a Party's lifecycle stage advances (Lead → Opportunity → Customer).
public sealed record PartyLifecycleAdvancedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid PartyId,
    string NewLifecycleStage) : IntegrationEvent(Id, OccurredOnUtc);
