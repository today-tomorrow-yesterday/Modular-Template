using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Inventory.Application.OnLotInventory.GetOnLotInventory;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Inventory.Presentation.Endpoints.V1.OnLotInventory;

internal sealed class GetOnLotInventoryEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapGet("/", GetOnLotInventoryAsync)
            .WithSummary("Get on-lot inventory by home center")
            .WithDescription("Returns all on-lot homes for a home center, enriched with land costs, ancillary data, and sale summary.")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<IReadOnlyCollection<OnLotInventoryResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> GetOnLotInventoryAsync(
        int? homeCenterNumber,
        ISender sender,
        CancellationToken cancellationToken)
    {
        if (homeCenterNumber is null or <= 0)
            return Results.Problem("homeCenterNumber query parameter is required and must be positive.", statusCode: StatusCodes.Status400BadRequest);

        var query = new GetOnLotInventoryQuery(homeCenterNumber.Value);

        var result = await sender.Send(query, cancellationToken);

        return result.Match(Results.Ok, ApiResults.Problem);
    }
}
