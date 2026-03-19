using Rtl.Core.Application.Messaging;

namespace Modules.Customer.Application.Customers.GetCustomerByPublicId;

public sealed record GetCustomerByPublicIdQuery(Guid PublicId) : IQuery<CustomerResponse>;

public sealed record CustomerResponse(
    Guid PublicId,
    string LifecycleStage,
    int HomeCenterNumber,
    string? FirstName,
    string? MiddleName,
    string? LastName,
    string? NameExtension,
    DateOnly? DateOfBirth,
    SalesAssignmentResponse[] SalesAssignments,
    Guid? CoBuyerPublicId,
    string? CoBuyerFirstName,
    string? CoBuyerMiddleName,
    string? CoBuyerLastName,
    DateOnly? CoBuyerDateOfBirth,
    ContactPointResponse[] ContactPoints,
    IdentifierResponse[] Identifiers,
    MailingAddressResponse? MailingAddress,
    string? SalesforceUrl,
    DateTime LastSyncedAtUtc);

public sealed record SalesAssignmentResponse(
    string Role,
    SalesPersonResponse SalesPerson);

public sealed record SalesPersonResponse(
    string Id,
    string Email,
    string Username,
    string FirstName,
    string LastName,
    int? HomeCenterNumber,
    string FederatedId);

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
