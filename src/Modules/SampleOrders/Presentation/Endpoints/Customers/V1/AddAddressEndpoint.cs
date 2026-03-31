using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.SampleOrders.Application.Customers.AddAddress;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.SampleOrders.Presentation.Endpoints.Customers.V1;

internal sealed class AddAddressEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPost("/{customerId:int}/addresses", AddAddressAsync)
            .WithName("AddCustomerAddress")
            .WithSummary("Add an address to a customer")
            .WithDescription("Adds a physical address to an existing customer.")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<ApiEnvelope<object>>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> AddAddressAsync(
        int customerId,
        AddAddressRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new AddAddressCommand(
            customerId,
            request.AddressLine1,
            request.AddressLine2,
            request.City,
            request.State,
            request.PostalCode,
            request.Country,
            request.IsPrimary);

        var result = await sender.Send(command, cancellationToken);

        return result.Match(
            () => ApiResponse.Success(),
            ApiResponse.Problem);
    }
}

public sealed record AddAddressRequest(
    string AddressLine1,
    string? AddressLine2,
    string City,
    string State,
    string PostalCode,
    string? Country,
    bool IsPrimary = false);
