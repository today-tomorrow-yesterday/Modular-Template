using Rtl.Core.Application.EventBus;

namespace Modules.Customer.IntegrationEvents;

// Published when a Party's home center assignment changes.
public sealed record PartyHomeCenterChangedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    int PartyId,
    Guid PartyPublicId,
    int NewHomeCenterNumber) : IntegrationEvent(Id, OccurredOnUtc);
