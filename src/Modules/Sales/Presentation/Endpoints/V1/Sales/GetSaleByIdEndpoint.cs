using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Sales.Application.Sales.GetSaleById;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.Presentation.Endpoints.V1.Sales;

internal sealed class GetSaleByIdEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapGet("/{publicSaleId:guid}", HandleAsync)
            .WithSummary("Get a sale by ID")
            .WithDescription("Retrieves sale details including party data and retail location.")
            .WithName("GetSaleById")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<ApiEnvelope<GetSaleByIdResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> HandleAsync(
        Guid publicSaleId,
        ISender sender,
        CancellationToken ct)
    {
        var query = new GetSaleByIdQuery(publicSaleId);

        var result = await sender.Send(query, ct);

        return result.Match(
            r => ApiResponse.Ok(new GetSaleByIdResponse(
                r.Id,
                r.SaleNumber,
                r.PartyId,
                new RetailLocationResponse(
                    r.RetailLocation.Id,
                    r.RetailLocation.Type,
                    r.RetailLocation.Name,
                    r.RetailLocation.StateCode,
                    r.RetailLocation.Zip,
                    r.RetailLocation.HomeCenterNumber),
                r.SaleType,
                r.SaleStatus,
                new SaleCustomerResponse(
                    r.Customer.RefCustomerId,
                    r.Customer.FirstName,
                    r.Customer.MiddleName,
                    r.Customer.LastName,
                    r.Customer.Email,
                    r.Customer.Phone,
                    r.Customer.MobilePhone,
                    r.Customer.HomePhone,
                    r.Customer.Birthdate,
                    r.Customer.HomeCenterNumber,
                    r.Customer.SalesforceId,
                    r.Customer.SalesforceUrl,
                    r.Customer.CoBuyerFirstName,
                    r.Customer.CoBuyerLastName,
                    r.Customer.CoBuyerBirthdate,
                    r.Customer.MailingAddress is not null
                        ? new MailingAddressResponse(
                            r.Customer.MailingAddress.Address,
                            r.Customer.MailingAddress.City,
                            r.Customer.MailingAddress.State,
                            r.Customer.MailingAddress.Zip)
                        : null,
                    r.Customer.PrimarySalesPersonFederatedId,
                    r.Customer.PrimarySalesPersonFirstName,
                    r.Customer.PrimarySalesPersonLastName,
                    r.Customer.SecondarySalesPersonFederatedId,
                    r.Customer.SecondarySalesPersonFirstName,
                    r.Customer.SecondarySalesPersonLastName),
                r.CreatedAtUtc)),
            ApiResponse.Problem);
    }
}

public sealed record GetSaleByIdResponse(
    Guid Id,
    int SaleNumber,
    Guid PartyId,
    RetailLocationResponse RetailLocation,
    string SaleType,
    string SaleStatus,
    SaleCustomerResponse Customer,
    DateTime CreatedAtUtc);

public sealed record RetailLocationResponse(
    int Id,
    string Type,
    string Name,
    string StateCode,
    string Zip,
    int? HomeCenterNumber);

public sealed record SaleCustomerResponse(
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
    MailingAddressResponse? MailingAddress,
    string? PrimarySalesPersonFederatedId,
    string? PrimarySalesPersonFirstName,
    string? PrimarySalesPersonLastName,
    string? SecondarySalesPersonFederatedId,
    string? SecondarySalesPersonFirstName,
    string? SecondarySalesPersonLastName);

public sealed record MailingAddressResponse(
    string? Address,
    string? City,
    string? State,
    string? Zip);
