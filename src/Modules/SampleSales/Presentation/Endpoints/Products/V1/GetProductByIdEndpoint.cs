using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.SampleSales.Application.Products.GetProduct;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.SampleSales.Presentation.Endpoints.Products.V1;

internal sealed class GetProductByIdEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapGet("/{productId:int}", GetProductByIdAsync)
            .WithName("GetProductById")
            .WithSummary("Get a product by ID")
            .WithDescription("Retrieves a product by its unique identifier.")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<ApiEnvelope<ProductResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> GetProductByIdAsync(
        int productId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetProductQuery(productId);

        var result = await sender.Send(query, cancellationToken);

        return result.Match(ApiResponse.Ok, ApiResponse.Problem);
    }
}
