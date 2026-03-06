using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Sales.Application.Insurance.GenerateWarrantyQuote;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.Presentation.Endpoints.V1.Insurance;

internal sealed class GenerateWarrantyQuoteEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPost("/{publicSaleId:guid}/insurance/quote/warranty", HandleAsync)
            .WithSummary("Generate warranty / extended warranty quote")
            .WithDescription("Calls iSeries adapter to generate an extended warranty (HBPP) quote. No request body — pricing is derived from the package's home and delivery details.")
            .WithName("GenerateWarrantyQuote")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<ApiEnvelope<WarrantyQuoteResponse>>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> HandleAsync(
        Guid publicSaleId,
        ISender sender,
        CancellationToken ct)
    {
        var command = new GenerateWarrantyQuoteCommand(publicSaleId);

        var result = await sender.Send(command, ct);

        return result.Match(
            r => ApiResponse.Ok(new WarrantyQuoteResponse(
                r.Premium,
                r.SalesTaxPremium,
                r.WarrantySelected)),
            ApiResponse.Problem);
    }
}

public sealed record WarrantyQuoteResponse(
    decimal Premium,
    decimal SalesTaxPremium,
    bool WarrantySelected);
