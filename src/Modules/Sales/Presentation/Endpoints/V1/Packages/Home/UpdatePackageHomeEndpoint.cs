using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Sales.Application.Packages.UpdatePackageHome;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.Presentation.Endpoints.V1.Packages.Home;

internal sealed class UpdatePackageHomeEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPut("/{publicPackageId:guid}/home", UpdatePackageHomeAsync)
            .WithSummary("Update package home section")
            .WithDescription("Adds or updates the home line on a package. PUT semantics — always replaces the existing home line.")
            .WithName("UpdatePackageHome")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<PackageUpdatedResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> UpdatePackageHomeAsync(
        Guid publicPackageId,
        UpdatePackageHomeRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdatePackageHomeCommand(publicPackageId, request);

        var result = await sender.Send(command, cancellationToken);

        return result.Match(
            r => Results.Ok(new PackageUpdatedResponse(
                r.GrossProfit,
                r.CommissionableGrossProfit,
                r.MustRecalculateTaxes)),
            ApiResults.Problem);
    }
}
