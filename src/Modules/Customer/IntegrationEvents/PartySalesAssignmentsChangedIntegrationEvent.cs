using Rtl.Core.Application.EventBus;

namespace Modules.Customer.IntegrationEvents;

// Published when a Person's sales assignments change (primary/supporting sales reps).
public sealed record PartySalesAssignmentsChangedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    int PartyId,
    Guid PartyPublicId,
    SalesAssignmentDto[] SalesAssignments) : IntegrationEvent(Id, OccurredOnUtc);
