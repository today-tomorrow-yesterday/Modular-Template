using Modules.Customer.Domain.Customers.Enums;
using Rtl.Core.Application.Messaging;

namespace Modules.Customer.Application.Customers.SyncCustomerFromCrm;

// CDC sync command — flattened (no PartyType, no PersonData/OrganizationData wrappers).
public sealed record SyncCustomerFromCrmCommand(
    int CrmPartyId,
    int HomeCenterNumber,
    LifecycleStage LifecycleStage,
    string? FirstName,
    string? MiddleName,
    string? LastName,
    string? NameExtension,
    DateOnly? DateOfBirth,
    SyncSalesAssignmentDto[] SalesAssignments,
    SyncContactPointDto[] ContactPoints,
    SyncIdentifierDto[] Identifiers,
    SyncMailingAddressDto? MailingAddress,
    string? SalesforceUrl,
    DateTimeOffset? CreatedOn,
    DateTimeOffset? LastModifiedOn) : ICommand;

public sealed record SyncSalesAssignmentDto(
    SalesAssignmentRole Role,
    SyncSalesPersonDto SalesPerson);

public sealed record SyncContactPointDto(
    ContactPointType Type,
    string Value,
    bool IsPrimary);

public sealed record SyncIdentifierDto(
    IdentifierType Type,
    string Value);

public sealed record SyncMailingAddressDto(
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? County,
    string? State,
    string? Country,
    string? PostalCode);

public sealed record SyncSalesPersonDto(
    string Id,
    string Email,
    string Username,
    string FirstName,
    string LastName,
    int? LotNumber,
    string FederatedId);
