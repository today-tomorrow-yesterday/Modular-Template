using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.SampleSales.Application.Catalogs.CreateCatalog;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.SampleSales.Presentation.Endpoints.Catalogs.V1;

internal sealed class CreateCatalogEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPost("/", CreateCatalogAsync)
            .WithSummary("Create a new catalog")
            .WithDescription("Creates a new catalog with the specified name and description.")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<ApiEnvelope<CreateCatalogResponse>>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> CreateCatalogAsync(
        CreateCatalogRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateCatalogCommand(request.Name, request.Description);

        var result = await sender.Send(command, cancellationToken);

        return result.Match(
            id => ApiResponse.Created($"/catalogs/{id}", new CreateCatalogResponse(id)),
            ApiResponse.Problem);
    }
}

public sealed record CreateCatalogRequest(string Name, string? Description);

public sealed record CreateCatalogResponse(Guid PublicId);
