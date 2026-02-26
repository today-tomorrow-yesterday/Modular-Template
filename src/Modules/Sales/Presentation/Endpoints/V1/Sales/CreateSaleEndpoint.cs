using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Sales.Application.Sales.CreateSale;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.Presentation.Endpoints.V1.Sales;

internal sealed class CreateSaleEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPost("/", HandleAsync)
            .WithSummary("Create a new sale")
            .WithDescription("Creates a new sale record with a party and home center assignment.")
            .WithName("CreateSale")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<CreateSaleResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithMetadata(new RequestBodyExample(Examples.Request));
    }

    internal static class Examples
    {
        public const string Request = """
        {
            "partyId": "6c49f440-2593-bc43-8534-4f4f35c8a666",
            "homeCenterNumber": 100
        }
        """;
    }

    private static async Task<IResult> HandleAsync(
        CreateSaleRequest request,
        ISender sender,
        CancellationToken ct)
    {
        var command = new CreateSaleCommand(request.PartyId, request.HomeCenterNumber);

        var result = await sender.Send(command, ct);

        return result.Match(
            r => Results.Created($"/api/v1/sales/{r.PublicId}", new CreateSaleResponse(r.PublicId, r.SaleNumber)),
            ApiResults.Problem);
    }
}

public sealed record CreateSaleRequest(
    Guid PartyId,
    int HomeCenterNumber);

public sealed record CreateSaleResponse(
    Guid Id,
    int SaleNumber);
