using Rtl.Core.Application.EventBus;

namespace Modules.Customer.IntegrationEvents;

// Published when a Customer's lifecycle stage advances (Lead → Opportunity → Customer).
[EventDetailType("rtl.customer.customerLifecycleAdvanced")]
public sealed record CustomerLifecycleAdvancedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid CustomerId,
    string NewLifecycleStage) : IntegrationEvent(Id, OccurredOnUtc);
