using Rtl.Core.Application.EventBus;

namespace Modules.Customer.IntegrationEvents;

// Published when a Person's co-buyer is assigned or removed.
public sealed record PartyCoBuyerChangedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    int PartyId,
    Guid PartyPublicId,
    int? CoBuyerPartyId,
    string? CoBuyerFirstName,
    string? CoBuyerLastName) : IntegrationEvent(Id, OccurredOnUtc);
