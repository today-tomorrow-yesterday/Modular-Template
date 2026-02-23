using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Sales.Application.Packages.UpdatePackageSalesTeam;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.Presentation.Endpoints.V1.Packages.SalesTeam;

internal sealed class UpdatePackageSalesTeamEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPut("/{publicPackageId:guid}/sales-team", HandleAsync)
            .WithSummary("Update package sales team")
            .WithDescription("Replaces the sales team. Pure data storage — does not affect summary fields.")
            .WithName("UpdatePackageSalesTeam")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<PackageUpdatedResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> HandleAsync(
        Guid publicPackageId,
        UpdatePackageSalesTeamMemberRequest[] members,
        ISender sender,
        CancellationToken ct)
    {
        var command = new UpdatePackageSalesTeamCommand(publicPackageId, members);

        var result = await sender.Send(command, ct);

        return result.Match(
            r => Results.Ok(new PackageUpdatedResponse(
                r.GrossProfit,
                r.CommissionableGrossProfit,
                r.MustRecalculateTaxes)),
            ApiResults.Problem);
    }
}
