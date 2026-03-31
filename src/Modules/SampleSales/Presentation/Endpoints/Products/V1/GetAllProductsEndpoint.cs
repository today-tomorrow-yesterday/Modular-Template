using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.SampleSales.Application.Products.GetProduct;
using Modules.SampleSales.Application.Products.GetProducts;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.SampleSales.Presentation.Endpoints.Products.V1;

internal sealed class GetAllProductsEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapGet("/", GetAllProductsAsync)
            .WithName("GetAllProducts")
            .WithSummary("Get all products")
            .WithDescription("Retrieves all products with optional limit.")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<ApiEnvelope<IReadOnlyCollection<ProductResponse>>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> GetAllProductsAsync(
        ISender sender,
        CancellationToken cancellationToken,
        int? limit = 100)
    {
        var query = new GetProductsQuery(limit);

        var result = await sender.Send(query, cancellationToken);

        return result.Match(ApiResponse.Ok, ApiResponse.Problem);
    }
}
