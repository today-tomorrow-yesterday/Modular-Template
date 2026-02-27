using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Inventory.Application.LandInventory.GetLandInventory;
using Rtl.Core.Domain.Results;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Inventory.Presentation.Endpoints.V1.LandInventory;

internal sealed class GetLandInventoryEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapGet("/", GetLandInventoryAsync)
            .WithSummary("Get land inventory by home center")
            .WithDescription("Returns all land parcels for a home center, filtered to allowed stock types.")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<ApiEnvelope<IReadOnlyCollection<LandInventoryResponse>>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> GetLandInventoryAsync(
        int? homeCenterNumber,
        ISender sender,
        CancellationToken cancellationToken)
    {
        if (homeCenterNumber is null or <= 0)
        {
            return ApiResponse.Problem(Error.Validation("HomeCenterNumber.Invalid", "homeCenterNumber query parameter is required and must be positive."));
        }

        var query = new GetLandInventoryQuery(homeCenterNumber.Value);

        var result = await sender.Send(query, cancellationToken);

        return result.Match(ApiResponse.Ok, ApiResponse.Problem);
    }
}
