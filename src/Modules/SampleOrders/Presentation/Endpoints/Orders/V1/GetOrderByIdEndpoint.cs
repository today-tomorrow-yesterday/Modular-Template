using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.SampleOrders.Application.Orders.GetOrder;
using ModularTemplate.Presentation.Endpoints;
using ModularTemplate.Presentation.Results;

namespace Modules.SampleOrders.Presentation.Endpoints.Orders.V1;

internal sealed class GetOrderByIdEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapGet("/{orderId:guid}", GetOrderByIdAsync)
            .WithName("GetOrderById")
            .WithSummary("Get an order by ID")
            .WithDescription("Retrieves an order by its unique identifier.")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<ApiEnvelope<OrderResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> GetOrderByIdAsync(
        Guid orderId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetOrderQuery(orderId);

        var result = await sender.Send(query, cancellationToken);

        return result.Match(ApiResponse.Ok, ApiResponse.Problem);
    }
}
