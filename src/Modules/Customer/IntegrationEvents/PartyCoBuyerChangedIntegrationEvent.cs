using Rtl.Core.Application.EventBus;

namespace Modules.Customer.IntegrationEvents;

// Published when a Person's co-buyer is assigned or removed.
public sealed record PartyCoBuyerChangedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid PartyId,
    Guid? CoBuyerPublicId,
    string? CoBuyerFirstName,
    string? CoBuyerLastName) : IntegrationEvent(Id, OccurredOnUtc);
