using Rtl.Core.Application.EventBus;

namespace Modules.Customer.IntegrationEvents;

// Published when a Customer's mailing address changes. No consumer today — available when needed.
[EventDetailType("rtl.customer.customerMailingAddressChanged")]
public sealed record CustomerMailingAddressChangedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid CustomerId,
    MailingAddressDto? MailingAddress) : IntegrationEvent(Id, OccurredOnUtc);
