using Rtl.Core.Application.EventBus;

namespace Modules.Customer.IntegrationEvents;

// Published when a Customer's home center assignment changes.
[EventDetailType("rtl.customer.customerHomeCenterChanged")]
public sealed record CustomerHomeCenterChangedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid PublicCustomerId,
    int NewHomeCenterNumber) : IntegrationEvent(Id, OccurredOnUtc);
