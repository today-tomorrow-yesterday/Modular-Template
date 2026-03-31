using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.SampleSales.Application.Products.CreateProduct;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.SampleSales.Presentation.Endpoints.Products.V1;

internal sealed class CreateProductEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPost("/", CreateProductAsync)
            .WithName("CreateProduct")
            .WithSummary("Create a new product")
            .WithDescription("Creates a new product with the specified name, description, price, and internal cost.")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<ApiEnvelope<CreateProductResponse>>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> CreateProductAsync(
        CreateProductRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateProductCommand(
            request.Name, 
            request.Description, 
            request.Price,
            request.InternalCost);

        var result = await sender.Send(command, cancellationToken);

        return result.Match(
            id => ApiResponse.Created($"/products/{id}", new CreateProductResponse(id)),
            ApiResponse.Problem);
    }
}

public sealed record CreateProductRequest(string Name, string? Description, decimal Price, decimal? InternalCost);

public sealed record CreateProductResponse(Guid PublicId);