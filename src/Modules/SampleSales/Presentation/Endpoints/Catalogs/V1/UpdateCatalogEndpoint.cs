using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.SampleSales.Application.Catalogs.UpdateCatalog;
using ModularTemplate.Presentation.Endpoints;
using ModularTemplate.Presentation.Results;

namespace Modules.SampleSales.Presentation.Endpoints.Catalogs.V1;

internal sealed class UpdateCatalogEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPut("/{catalogId:int}", UpdateCatalogAsync)
            .WithMetadata(new RequestBodyExample("""{ "name": "Summer Collection 2026 - Updated", "description": "Revised seasonal product catalog with new arrivals" }"""))
            .WithName("UpdateCatalog")
            .WithSummary("Update a catalog")
            .WithDescription("Updates an existing catalog with the specified details.")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<ApiEnvelope<object>>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> UpdateCatalogAsync(
        int catalogId,
        UpdateCatalogRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCatalogCommand(
            catalogId,
            request.Name,
            request.Description);

        var result = await sender.Send(command, cancellationToken);

        return result.Match(
            () => ApiResponse.Success(),
            ApiResponse.Problem);
    }
}

public sealed record UpdateCatalogRequest(string Name, string? Description);
