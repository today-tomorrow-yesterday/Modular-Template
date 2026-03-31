using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.SampleOrders.Application.Orders.UpdateOrderStatus;
using Modules.SampleOrders.Domain.Orders;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.SampleOrders.Presentation.Endpoints.Orders.V1;

internal sealed class UpdateOrderStatusEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPatch("/{orderId:int}/status", UpdateOrderStatusAsync)
            .WithName("UpdateOrderStatus")
            .WithSummary("Update order status")
            .WithDescription("Updates the status of an existing order.")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<ApiEnvelope<object>>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> UpdateOrderStatusAsync(
        int orderId,
        UpdateOrderStatusRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateOrderStatusCommand(orderId, request.Status);

        var result = await sender.Send(command, cancellationToken);

        return result.Match(
            () => ApiResponse.Success(),
            ApiResponse.Problem);
    }
}

public sealed record UpdateOrderStatusRequest(OrderStatus Status);
