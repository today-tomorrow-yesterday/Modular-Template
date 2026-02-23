using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Sales.Application.Packages.GetPackageById;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.Presentation.Endpoints.V1.Packages;

internal sealed class GetPackageByIdEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapGet("/{publicSaleId:guid}/packages/{publicPackageId:guid}", HandleAsync)
            .WithSummary("Get a specific package by ID")
            .WithDescription("Returns a package with all sections and funding info.")
            .WithName("GetPackageById")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<PackageDetailResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> HandleAsync(
        Guid publicSaleId,
        Guid publicPackageId,
        ISender sender,
        CancellationToken ct)
    {
        var query = new GetPackageByIdQuery(publicPackageId);

        var result = await sender.Send(query, ct);

        return result.Match(
            Results.Ok,
            ApiResults.Problem);
    }
}
