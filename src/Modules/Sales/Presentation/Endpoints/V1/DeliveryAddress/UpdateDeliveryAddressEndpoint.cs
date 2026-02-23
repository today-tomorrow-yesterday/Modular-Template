using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Sales.Application.DeliveryAddresses.UpdateDeliveryAddress;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.Presentation.Endpoints.V1.DeliveryAddress;

internal sealed class UpdateDeliveryAddressEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPut("/{publicSaleId:guid}/delivery-address", HandleAsync)
            .WithSummary("Update delivery address for a sale")
            .WithDescription("Updates the delivery address. Triggers tax/insurance cascades on state or occupancy changes.")
            .WithName("UpdateDeliveryAddress")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> HandleAsync(
        Guid publicSaleId,
        UpdateDeliveryAddressRequest request,
        ISender sender,
        CancellationToken ct)
    {
        var command = new UpdateDeliveryAddressCommand(
            publicSaleId,
            request.OccupancyType,
            request.IsWithinCityLimits,
            request.AddressLine1,
            request.City,
            request.County,
            request.State,
            request.PostalCode);

        var result = await sender.Send(command, ct);

        return result.Match(
            () => Results.NoContent(),
            ApiResults.Problem);
    }
}

public sealed record UpdateDeliveryAddressRequest(
    string? OccupancyType,
    bool IsWithinCityLimits,
    string? AddressLine1,
    string? City,
    string? County,
    string? State,
    string? PostalCode);
