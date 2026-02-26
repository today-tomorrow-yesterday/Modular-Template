using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Sales.Application.Packages.UpdatePackageName;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.Presentation.Endpoints.V1.Packages;

internal sealed class UpdatePackageNameEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPatch("/{publicPackageId:guid}/name", HandleAsync)
            .WithSummary("Rename a package")
            .WithDescription("Updates only the display name of a package.")
            .WithName("UpdatePackageName")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithMetadata(new RequestBodyExample(Examples.Request));
    }

    private static async Task<IResult> HandleAsync(
        Guid publicPackageId,
        UpdatePackageNameRequest request,
        ISender sender,
        CancellationToken ct)
    {
        var command = new UpdatePackageNameCommand(publicPackageId, request.Name);

        var result = await sender.Send(command, ct);

        return result.Match(() => Results.NoContent(), ApiResults.Problem);
    }

    internal static class Examples
    {
        public const string Request = """
        {
            "name": "Alternate"
        }
        """;
    }
}

public sealed record UpdatePackageNameRequest(string Name);
