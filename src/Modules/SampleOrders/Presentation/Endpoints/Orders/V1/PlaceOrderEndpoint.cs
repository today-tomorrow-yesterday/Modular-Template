using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.SampleOrders.Application.Orders.PlaceOrder;
using ModularTemplate.Presentation.Endpoints;
using ModularTemplate.Presentation.Results;

namespace Modules.SampleOrders.Presentation.Endpoints.Orders.V1;

internal sealed class PlaceOrderEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPost("/", PlaceOrderAsync)
            .WithMetadata(new RequestBodyExample("""{ "publicCustomerId": "01970f2e-0000-7000-8000-000000000002", "productCacheId": 1, "quantity": 2 }"""))
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
        var command = new PlaceOrderCommand(request.PublicCustomerId, request.ProductCacheId, request.Quantity);

        var result = await sender.Send(command, cancellationToken);

        return result.Match(
            publicId => ApiResponse.Created($"/orders/{publicId}", new PlaceOrderResponse(publicId)),
            ApiResponse.Problem);
    }
}

public sealed record PlaceOrderRequest(Guid PublicCustomerId, int ProductCacheId, int Quantity);

public sealed record PlaceOrderResponse(Guid PublicId);
