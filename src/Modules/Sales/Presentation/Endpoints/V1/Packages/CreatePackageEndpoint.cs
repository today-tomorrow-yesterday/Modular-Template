using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Sales.Application.Packages.CreatePackage;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.Presentation.Endpoints.V1.Packages;

internal sealed class CreatePackageEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPost("/{publicSaleId:guid}/packages", HandleAsync)
            .WithSummary("Create a new package for a sale")
            .WithDescription("Creates a new package on the sale. The first package is automatically set as primary.")
            .WithName("CreatePackage")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<ApiEnvelope<CreatePackageResponse>>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithMetadata(new RequestBodyExample(Examples.Request));
    }

    private static async Task<IResult> HandleAsync(
        Guid publicSaleId,
        CreatePackageRequest request,
        ISender sender,
        CancellationToken ct)
    {
        var command = new CreatePackageCommand(publicSaleId, request.Name);

        var result = await sender.Send(command, ct);

        return result.Match(
            r => ApiResponse.Created($"/api/v1/sales/{publicSaleId}/packages/{r.PublicId}", new CreatePackageResponse(r.PublicId)),
            ApiResponse.Problem);
    }

    internal static class Examples
    {
        public const string Request = """
        {
            "name": "Primary"
        }
        """;
    }
}

public sealed record CreatePackageRequest(string Name);

public sealed record CreatePackageResponse(Guid Id);
