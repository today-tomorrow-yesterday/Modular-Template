using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Sales.Application.Pricing.GetOptionTotals;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.Presentation.Endpoints.V1.Pricing;

internal sealed class GetOptionTotalsEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapGet("/{publicSaleId:guid}/pricing/option-totals", HandleAsync)
            .WithSummary("Get option totals for a sale")
            .WithDescription("Proxies to iSeries POST /v1/pricing/option-totals via adapter. For quoted homes only.")
            .WithName("GetOptionTotals")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<OptionTotalsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> HandleAsync(
        Guid publicSaleId,
        int plantNumber,
        int quoteNumber,
        int orderNumber,
        string effectiveDate,
        ISender sender,
        CancellationToken ct)
    {
        var query = new GetOptionTotalsQuery(
            publicSaleId,
            plantNumber,
            quoteNumber,
            orderNumber,
            effectiveDate);

        var result = await sender.Send(query, ct);

        return result.Match(
            r => Results.Ok(new OptionTotalsResponse(r.HbgOptionTotal, r.RetailOptionTotal)),
            ApiResults.Problem);
    }
}

public sealed record OptionTotalsResponse(
    decimal HbgOptionTotal,
    decimal RetailOptionTotal);
