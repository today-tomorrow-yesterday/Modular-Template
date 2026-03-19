using Rtl.Core.Application.EventBus;

namespace Modules.Customer.IntegrationEvents;

// Published when a Customer's name changes.
[EventDetailType("rtl.customer.customerNameChanged")]
public sealed record CustomerNameChangedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid CustomerId,
    string? FirstName,
    string? MiddleName,
    string? LastName,
    string? NameExtension) : IntegrationEvent(Id, OccurredOnUtc);
