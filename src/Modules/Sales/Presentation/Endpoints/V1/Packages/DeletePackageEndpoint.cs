using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Sales.Application.Packages.DeletePackage;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.Presentation.Endpoints.V1.Packages;

internal sealed class DeletePackageEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapDelete("/{publicPackageId:guid}", HandleAsync)
            .WithSummary("Delete a package")
            .WithDescription("Deletes a package and all its line items. Cannot delete the primary package if other packages exist.")
            .WithName("DeletePackage")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<ApiEnvelope<object>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> HandleAsync(
        Guid publicPackageId,
        ISender sender,
        CancellationToken ct)
    {
        var command = new DeletePackageCommand(publicPackageId);

        var result = await sender.Send(command, ct);

        return result.Match(
            () => ApiResponse.Success(),
            ApiResponse.Problem);
    }
}
