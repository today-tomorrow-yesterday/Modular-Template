using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Sales.Application.Packages.GetPackagesBySale;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.Presentation.Endpoints.V1.Packages;

internal sealed class GetPackagesBySaleEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapGet("/{publicSaleId:guid}/packages", HandleAsync)
            .WithSummary("Get all packages for a sale")
            .WithDescription("Returns all packages for a sale (summaries only).")
            .WithName("GetPackagesBySale")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<ApiEnvelope<IReadOnlyCollection<PackageSummaryResponse>>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> HandleAsync(
        Guid publicSaleId,
        ISender sender,
        CancellationToken ct)
    {
        var query = new GetPackagesBySaleQuery(publicSaleId);

        var result = await sender.Send(query, ct);

        return result.Match(
            ApiResponse.Ok,
            ApiResponse.Problem);
    }
}
