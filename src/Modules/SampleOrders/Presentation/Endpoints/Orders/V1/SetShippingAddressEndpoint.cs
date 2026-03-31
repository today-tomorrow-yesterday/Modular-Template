using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.SampleOrders.Application.Orders.SetShippingAddress;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.SampleOrders.Presentation.Endpoints.Orders.V1;

internal sealed class SetShippingAddressEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPut("/{orderId:int}/shipping-address", SetShippingAddressAsync)
            .WithName("SetShippingAddress")
            .WithSummary("Set shipping address")
            .WithDescription("Sets or updates the shipping address for an order.")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<ApiEnvelope<object>>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> SetShippingAddressAsync(
        int orderId,
        SetShippingAddressRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new SetShippingAddressCommand(
            orderId,
            request.AddressLine1,
            request.AddressLine2,
            request.City,
            request.State,
            request.PostalCode,
            request.Country);

        var result = await sender.Send(command, cancellationToken);

        return result.Match(
            () => ApiResponse.Success(),
            ApiResponse.Problem);
    }
}

public sealed record SetShippingAddressRequest(
    string AddressLine1,
    string? AddressLine2,
    string City,
    string State,
    string PostalCode,
    string? Country);
