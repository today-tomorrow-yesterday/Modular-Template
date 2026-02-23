using Rtl.Core.Application.EventBus;

namespace Modules.Customer.IntegrationEvents;

// Published when a Party's contact points change (email, phone, etc.).
public sealed record PartyContactPointsChangedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    int PartyId,
    Guid PartyPublicId,
    ContactPointDto[] ContactPoints) : IntegrationEvent(Id, OccurredOnUtc);
