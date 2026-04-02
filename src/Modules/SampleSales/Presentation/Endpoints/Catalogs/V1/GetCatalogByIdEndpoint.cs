using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.SampleSales.Application.Catalogs.GetCatalog;
using ModularTemplate.Presentation.Endpoints;
using ModularTemplate.Presentation.Results;

namespace Modules.SampleSales.Presentation.Endpoints.Catalogs.V1;

internal sealed class GetCatalogByIdEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapGet("/{catalogId:guid}", GetCatalogByIdAsync)
            .WithName("GetCatalogById")
            .WithSummary("Get a catalog by ID")
            .WithDescription("Retrieves a catalog by its unique identifier.")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<ApiEnvelope<CatalogResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> GetCatalogByIdAsync(
        Guid catalogId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetCatalogQuery(catalogId);

        var result = await sender.Send(query, cancellationToken);

        return result.Match(ApiResponse.Ok, ApiResponse.Problem);
    }
}
