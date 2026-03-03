using Rtl.Core.Application.EventBus;

namespace Modules.Customer.IntegrationEvents;

// Published when a Party's mailing address changes. No consumer today — available when needed.
public sealed record PartyMailingAddressChangedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid PartyId,
    MailingAddressDto? MailingAddress) : IntegrationEvent(Id, OccurredOnUtc);
