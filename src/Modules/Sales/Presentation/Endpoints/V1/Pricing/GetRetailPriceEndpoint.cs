using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Sales.Application.Pricing.GetRetailPrice;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.Presentation.Endpoints.V1.Pricing;

internal sealed class GetRetailPriceEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapGet("/{publicSaleId:guid}/pricing/retail-price", HandleAsync)
            .WithSummary("Get retail price for a sale")
            .WithDescription("Proxies to iSeries POST /v1/pricing/retail via adapter.")
            .WithName("GetRetailPrice")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<RetailPriceResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> HandleAsync(
        Guid publicSaleId,
        string serialNumber,
        decimal invoiceTotal,
        int numberOfAxles,
        decimal hbgOptionTotal,
        decimal retailOptionTotal,
        string modelNumber,
        decimal baseCost,
        string effectiveDate,
        ISender sender,
        CancellationToken ct)
    {
        var query = new GetRetailPriceQuery(
            publicSaleId,
            serialNumber,
            invoiceTotal,
            numberOfAxles,
            hbgOptionTotal,
            retailOptionTotal,
            modelNumber,
            baseCost,
            effectiveDate);

        var result = await sender.Send(query, ct);

        return result.Match(
            price => Results.Ok(new RetailPriceResponse(price)),
            ApiResults.Problem);
    }
}

public sealed record RetailPriceResponse(decimal RetailPrice);
