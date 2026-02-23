using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Sales.Application.Packages.UpdatePackageConcessions;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.Presentation.Endpoints.V1.Packages.Concessions;

internal sealed class UpdatePackageConcessionsEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPut("/{publicPackageId:guid}/concessions", HandleAsync)
            .WithSummary("Update package concessions section")
            .WithDescription("Upserts the concessions amount. 0 removes existing line and associated project cost.")
            .WithName("UpdatePackageConcessions")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<PackageUpdatedResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> HandleAsync(
        Guid publicPackageId,
        UpdatePackageConcessionsRequest request,
        ISender sender,
        CancellationToken ct)
    {
        var command = new UpdatePackageConcessionsCommand(publicPackageId, request.Amount);

        var result = await sender.Send(command, ct);

        return result.Match(
            r => Results.Ok(new PackageUpdatedResponse(
                r.GrossProfit,
                r.CommissionableGrossProfit,
                r.MustRecalculateTaxes)),
            ApiResults.Problem);
    }
}

public sealed record UpdatePackageConcessionsRequest(decimal Amount);
