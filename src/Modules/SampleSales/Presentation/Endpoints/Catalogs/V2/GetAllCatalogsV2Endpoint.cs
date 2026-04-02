using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.SampleSales.Application.Catalogs.GetCatalog;
using Modules.SampleSales.Application.Catalogs.GetCatalogs;
using Modules.SampleSales.Application.FeatureManagement;
using ModularTemplate.Presentation.Endpoints;
using ModularTemplate.Presentation.FeatureManagement;
using ModularTemplate.Presentation.Results;

namespace Modules.SampleSales.Presentation.Endpoints.Catalogs.V2;

/// <summary>
/// V2 endpoint: Returns catalogs with pagination metadata.
/// Demonstrates API versioning with enhanced response format.
/// </summary>
internal sealed class GetAllCatalogsV2Endpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapGet("/", GetAllCatalogsAsync)
            .WithName("GetAllCatalogsV2")
            .WithSummary("Get all catalogs (v2)")
            .WithDescription("Retrieves catalogs with pagination metadata. Enhanced response format with total count and page info.")
            .MapToApiVersion(new ApiVersion(2, 0))
            .Produces<ApiEnvelope<PagedCatalogResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .RequireFeature(SampleSalesFeatures.CatalogV2Pagination);
    }

    private static async Task<IResult> GetAllCatalogsAsync(
        ISender sender,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 20)
    {
        // Calculate offset for pagination
        var offset = (page - 1) * pageSize;

        var query = new GetCatalogsQuery(pageSize + 1, offset); // Fetch one extra to check if there's more

        var result = await sender.Send(query, cancellationToken);

        return result.Match(
            catalogs =>
            {
                var hasMore = catalogs.Count > pageSize;
                var items = catalogs.Take(pageSize).ToList();

                var response = new PagedCatalogResponse(
                    Items: items,
                    Pagination: new PaginationMetadata(
                        Page: page,
                        PageSize: pageSize,
                        TotalItems: items.Count,
                        HasNextPage: hasMore,
                        HasPreviousPage: page > 1));

                return ApiResponse.Ok(response);
            },
            ApiResponse.Problem);
    }
}

/// <summary>
/// V2 response format with pagination metadata.
/// </summary>
public sealed record PagedCatalogResponse(
    IReadOnlyCollection<CatalogResponse> Items,
    PaginationMetadata Pagination);

/// <summary>
/// Pagination metadata for paginated responses.
/// </summary>
public sealed record PaginationMetadata(
    int Page,
    int PageSize,
    int TotalItems,
    bool HasNextPage,
    bool HasPreviousPage);
