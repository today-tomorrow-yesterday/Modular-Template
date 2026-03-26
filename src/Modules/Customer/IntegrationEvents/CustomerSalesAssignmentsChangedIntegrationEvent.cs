using Rtl.Core.Application.EventBus;

namespace Modules.Customer.IntegrationEvents;

// Published when a Customer's sales assignments change (primary/supporting sales reps).
[EventDetailType("rtl.customer.customerSalesAssignmentsChanged")]
public sealed record CustomerSalesAssignmentsChangedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid PublicCustomerId,
    SalesAssignmentDto[] SalesAssignments) : IntegrationEvent(Id, OccurredOnUtc);
