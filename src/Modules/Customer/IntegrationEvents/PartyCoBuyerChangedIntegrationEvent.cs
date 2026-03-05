using Rtl.Core.Application.EventBus;

namespace Modules.Customer.IntegrationEvents;

// Published when a Person's co-buyer is assigned or removed.
[EventDetailType("rtl.customer.partyCoBuyerChanged")]
public sealed record PartyCoBuyerChangedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid PartyId,
    Guid? CoBuyerPublicId,
    string? CoBuyerFirstName,
    string? CoBuyerLastName) : IntegrationEvent(Id, OccurredOnUtc);
