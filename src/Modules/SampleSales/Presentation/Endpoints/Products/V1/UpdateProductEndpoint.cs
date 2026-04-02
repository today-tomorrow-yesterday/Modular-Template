using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.SampleSales.Application.Products.UpdateProduct;
using ModularTemplate.Presentation.Endpoints;
using ModularTemplate.Presentation.Results;

namespace Modules.SampleSales.Presentation.Endpoints.Products.V1;

internal sealed class UpdateProductEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPut("/{productId:guid}", UpdateProductAsync)
            .WithMetadata(new RequestBodyExample("""{ "name": "Wireless Keyboard Pro", "description": "Ergonomic wireless keyboard with RGB backlit keys", "price": 89.99, "isActive": true }"""))
            .WithName("UpdateProduct")
            .WithSummary("Update a product")
            .WithDescription("Updates an existing product with the specified details.")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<ApiEnvelope<object>>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> UpdateProductAsync(
        Guid productId,
        UpdateProductRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProductCommand(
            productId,
            request.Name,
            request.Description,
            request.Price,
            request.IsActive);

        var result = await sender.Send(command, cancellationToken);

        return result.Match(
            () => ApiResponse.Success(),
            ApiResponse.Problem);
    }
}

public sealed record UpdateProductRequest(string Name, string? Description, decimal Price, bool IsActive);
