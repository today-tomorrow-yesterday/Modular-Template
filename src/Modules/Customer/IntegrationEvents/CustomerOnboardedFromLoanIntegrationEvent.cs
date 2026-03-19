using Rtl.Core.Application.EventBus;

namespace Modules.Customer.IntegrationEvents;

// ECST integration event — Customer onboarded from VMF LOS loan deal.
// Always Person-only. No SalesAssignments. No CoBuyer.
// Funding correlates via LoanId in Identifiers[].
[EventDetailType("rtl.customer.customerOnboardedFromLoan")]
public sealed record CustomerOnboardedFromLoanIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid CustomerId,
    int HomeCenterNumber,
    string? FirstName,
    string? MiddleName,
    string? LastName,
    string? NameExtension,
    DateOnly? DateOfBirth,
    ContactPointDto[] ContactPoints,
    IdentifierDto[] Identifiers) : IntegrationEvent(Id, OccurredOnUtc);
