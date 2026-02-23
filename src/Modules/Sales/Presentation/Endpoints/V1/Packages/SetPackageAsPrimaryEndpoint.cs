using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Sales.Application.Packages.SetPackageAsPrimary;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.Presentation.Endpoints.V1.Packages;

internal sealed class SetPackageAsPrimaryEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPatch("/{publicPackageId:guid}", HandleAsync)
            .WithSummary("Set a package as the primary package")
            .WithDescription("Marks this package as primary. Idempotent. Use query parameter action=set-as-primary.")
            .WithName("SetPackageAsPrimary")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> HandleAsync(
        Guid publicPackageId,
        string action,
        ISender sender,
        CancellationToken ct)
    {
        var command = new SetPackageAsPrimaryCommand(publicPackageId);

        var result = await sender.Send(command, ct);

        return result.Match(
            () => Results.NoContent(),
            ApiResults.Problem);
    }
}
