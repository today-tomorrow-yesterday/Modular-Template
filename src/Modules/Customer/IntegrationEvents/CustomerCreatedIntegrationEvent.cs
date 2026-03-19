using Rtl.Core.Application.EventBus;

namespace Modules.Customer.IntegrationEvents;

// ECST integration event — full Customer state for consumer caching.
[EventDetailType("rtl.customer.customerCreated")]
public sealed record CustomerCreatedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid CustomerId,
    string LifecycleStage,
    int HomeCenterNumber,
    string? FirstName,
    string? MiddleName,
    string? LastName,
    string? NameExtension,
    DateOnly? DateOfBirth,
    SalesAssignmentDto[] SalesAssignments,
    Guid? CoBuyerPublicId,
    string? CoBuyerFirstName,
    string? CoBuyerMiddleName,
    string? CoBuyerLastName,
    DateOnly? CoBuyerDateOfBirth,
    ContactPointDto[] ContactPoints,
    IdentifierDto[] Identifiers,
    MailingAddressDto? MailingAddress,
    string? SalesforceUrl) : IntegrationEvent(Id, OccurredOnUtc);

public sealed record SalesAssignmentDto(
    string Role,
    string SalesPersonId,
    string Email,
    string Username,
    string FirstName,
    string LastName,
    int? HomeCenterNumber,
    string FederatedId);

public sealed record ContactPointDto(string Type, string Value, bool IsPrimary);

public sealed record IdentifierDto(string Type, string Value);

public sealed record MailingAddressDto(
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? County,
    string? State,
    string? Country,
    string? PostalCode);
