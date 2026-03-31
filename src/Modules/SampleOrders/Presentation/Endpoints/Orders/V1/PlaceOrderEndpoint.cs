using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.SampleOrders.Application.Orders.PlaceOrder;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.SampleOrders.Presentation.Endpoints.Orders.V1;

internal sealed class PlaceOrderEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPost("/", PlaceOrderAsync)
            .WithName("PlaceOrder")
            .WithSummary("Place a new order")
            .WithDescription("Places a new order for a product.")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<ApiEnvelope<PlaceOrderResponse>>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> PlaceOrderAsync(
        PlaceOrderRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new PlaceOrderCommand(request.CustomerId, request.ProductCacheId, request.Quantity);

        var result = await sender.Send(command, cancellationToken);

        return result.Match(
            publicId => ApiResponse.Created($"/orders/{publicId}", new PlaceOrderResponse(publicId)),
            ApiResponse.Problem);
    }
}

public sealed record PlaceOrderRequest(int CustomerId, int ProductCacheId, int Quantity);

public sealed record PlaceOrderResponse(Guid PublicId);
