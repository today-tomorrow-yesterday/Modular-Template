using Rtl.Core.Application.Messaging;

namespace Modules.Customer.Application.Parties.GetPartyByPublicId;

public sealed record GetPartyByPublicIdQuery(Guid PublicId) : IQuery<PartyResponse>;

// Polymorphic response — PartyType discriminates which sub-DTO is populated.
public sealed record PartyResponse(
    Guid PublicId,
    string PartyType,
    string LifecycleStage,
    int HomeCenterNumber,
    PersonDataResponse? PersonData,
    OrganizationDataResponse? OrganizationData,
    ContactPointResponse[] ContactPoints,
    IdentifierResponse[] Identifiers,
    MailingAddressResponse? MailingAddress,
    string? SalesforceUrl,
    DateTime LastSyncedAtUtc);

public sealed record PersonDataResponse(
    string? FirstName,
    string? MiddleName,
    string? LastName,
    string? NameExtension,
    DateOnly? DateOfBirth,
    SalesAssignmentResponse[] SalesAssignments,
    int? CoBuyerPartyId,
    Guid? CoBuyerPublicId,
    // Flattened CoBuyer fields for BFF backward compatibility
    string? CoBuyerFirstName,
    string? CoBuyerMiddleName,
    string? CoBuyerLastName,
    DateOnly? CoBuyerDateOfBirth);

public sealed record SalesAssignmentResponse(
    string Role,
    SalesPersonResponse SalesPerson);

public sealed record OrganizationDataResponse(
    string? OrganizationName);

public sealed record ContactPointResponse(
    string Type,
    string Value,
    bool IsPrimary);

public sealed record IdentifierResponse(
    string Type,
    string Value);

public sealed record MailingAddressResponse(
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? County,
    string? State,
    string? Country,
    string? PostalCode);

public sealed record SalesPersonResponse(
    string Id,
    string Email,
    string Username,
    string FirstName,
    string LastName,
    int? LotNumber,
    string FederatedId);
