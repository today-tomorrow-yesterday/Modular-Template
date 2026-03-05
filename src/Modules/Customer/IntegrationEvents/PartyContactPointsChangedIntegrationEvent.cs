using Rtl.Core.Application.EventBus;

namespace Modules.Customer.IntegrationEvents;

// Published when a Party's contact points change (email, phone, etc.).
[EventDetailType("rtl.customer.partyContactPointsChanged")]
public sealed record PartyContactPointsChangedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid PartyId,
    ContactPointDto[] ContactPoints) : IntegrationEvent(Id, OccurredOnUtc);
