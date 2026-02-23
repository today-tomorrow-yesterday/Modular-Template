using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Sales.Application.Pricing.GetWheelsAndAxlesPriceByStock;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.Presentation.Endpoints.V1.Pricing;

internal sealed class GetWheelsAndAxlesPriceByStockEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapGet("/{publicSaleId:guid}/pricing/wheels-and-axles-by-stock", HandleAsync)
            .WithSummary("Get wheels and axles price by stock number")
            .WithDescription("Calculates W&A price by stock number via iSeries adapter. IsRetaining determines rent (Cat 1/Item 28) vs purchase (Cat 1/Item 29).")
            .WithName("GetWheelsAndAxlesPriceByStock")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<WheelsAndAxlesPriceByStockResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> HandleAsync(
        Guid publicSaleId,
        string stockNumber,
        bool isRetaining,
        ISender sender,
        CancellationToken ct)
    {
        var query = new GetWheelsAndAxlesPriceByStockQuery(publicSaleId, stockNumber, isRetaining);

        var result = await sender.Send(query, ct);

        return result.Match(
            price => Results.Ok(new WheelsAndAxlesPriceByStockResponse(price)),
            ApiResults.Problem);
    }
}

public sealed record WheelsAndAxlesPriceByStockResponse(decimal WheelsAndAxlesPrice);
