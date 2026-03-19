using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.Sales.GetSaleById;

public sealed record GetSaleByIdQuery(Guid SalePublicId) : IQuery<GetSaleByIdResult>;

public sealed record GetSaleByIdResult(
    Guid Id,
    int SaleNumber,
    Guid CustomerId,
    RetailLocationResult RetailLocation,
    string SaleType,
    string SaleStatus,
    SaleCustomerResult Customer,
    DateTime CreatedAtUtc);

public sealed record RetailLocationResult(
    int Id,
    string Type,
    string Name,
    string StateCode,
    string Zip,
    int? HomeCenterNumber);

public sealed record SaleCustomerResult(
    Guid RefCustomerId,
    string FirstName,
    string? MiddleName,
    string LastName,
    string? Email,
    string? Phone,
    string? MobilePhone,
    string? HomePhone,
    string? Birthdate,
    int HomeCenterNumber,
    string? SalesforceId,
    string? SalesforceUrl,
    string? CoBuyerFirstName,
    string? CoBuyerLastName,
    string? CoBuyerBirthdate,
    MailingAddressResult? MailingAddress,
    string? PrimarySalesPersonFederatedId,
    string? PrimarySalesPersonFirstName,
    string? PrimarySalesPersonLastName,
    string? SecondarySalesPersonFederatedId,
    string? SecondarySalesPersonFirstName,
    string? SecondarySalesPersonLastName);

public sealed record MailingAddressResult(
    string? Address,
    string? City,
    string? State,
    string? Zip);
