using Rtl.Core.Application.EventBus;

namespace Modules.Customer.IntegrationEvents;

// Published when a Party's home center assignment changes.
public sealed record PartyHomeCenterChangedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid PartyId,
    int NewHomeCenterNumber) : IntegrationEvent(Id, OccurredOnUtc);
