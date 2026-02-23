using Rtl.Core.Application.EventBus;

namespace Modules.Customer.IntegrationEvents;

// ECST integration event — Person onboarded from VMF LOS loan deal.
// Always Person-only. No SalesAssignments. No CoBuyer.
// Funding correlates via LoanId in Identifiers[].
public sealed record PartyOnboardedFromLoanIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    int PartyId,
    Guid PublicId,
    int HomeCenterNumber,
    string? FirstName,
    string? MiddleName,
    string? LastName,
    string? NameExtension,
    DateOnly? DateOfBirth,
    ContactPointDto[] ContactPoints,
    IdentifierDto[] Identifiers) : IntegrationEvent(Id, OccurredOnUtc);
