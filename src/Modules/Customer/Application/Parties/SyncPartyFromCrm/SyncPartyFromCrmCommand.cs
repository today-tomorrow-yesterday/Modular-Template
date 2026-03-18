using Modules.Customer.Domain.Parties.Enums;
using Rtl.Core.Application.Messaging;

namespace Modules.Customer.Application.Parties.SyncPartyFromCrm;

// CDC sync command — handles both Person and Organization from Salesforce CDC.
// PartyType discriminates which sub-DTO is populated.
public sealed record SyncPartyFromCrmCommand(
    int CrmPartyId,
    PartyType PartyType,
    int HomeCenterNumber,
    LifecycleStage LifecycleStage,
    SyncPersonDataDto? PersonData,
    SyncOrganizationDataDto? OrganizationData,
    SyncContactPointDto[] ContactPoints,
    SyncIdentifierDto[] Identifiers,
    SyncMailingAddressDto? MailingAddress,
    string? SalesforceUrl,
    DateTimeOffset? CreatedOn,
    DateTimeOffset? LastModifiedOn) : ICommand;

public sealed record SyncPersonDataDto(
    string? FirstName,
    string? MiddleName,
    string? LastName,
    string? NameExtension,
    DateOnly? DateOfBirth,
    SyncSalesAssignmentDto[] SalesAssignments);

public sealed record SyncSalesAssignmentDto(
    SalesAssignmentRole Role,
    SyncSalesPersonDto SalesPerson);

public sealed record SyncOrganizationDataDto(
    string? OrganizationName);

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
