using Rtl.Core.Application.EventBus;

namespace Modules.Customer.IntegrationEvents;

// Published when a Customer's contact points change (email, phone, etc.).
[EventDetailType("rtl.customer.customerContactPointsChanged")]
public sealed record CustomerContactPointsChangedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid PublicCustomerId,
    ContactPointDto[] ContactPoints) : IntegrationEvent(Id, OccurredOnUtc);
