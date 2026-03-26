using Rtl.Core.Application.EventBus;

namespace Modules.Customer.IntegrationEvents;

// Published when a Customer's co-buyer is assigned or removed.
[EventDetailType("rtl.customer.customerCoBuyerChanged")]
public sealed record CustomerCoBuyerChangedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid PublicCustomerId,
    Guid? CoBuyerPublicId,
    string? CoBuyerFirstName,
    string? CoBuyerLastName) : IntegrationEvent(Id, OccurredOnUtc);
