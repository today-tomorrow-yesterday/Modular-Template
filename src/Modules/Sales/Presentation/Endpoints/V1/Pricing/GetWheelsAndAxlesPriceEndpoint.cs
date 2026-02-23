using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Sales.Application.Pricing.GetWheelsAndAxlesPrice;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.Presentation.Endpoints.V1.Pricing;

internal sealed class GetWheelsAndAxlesPriceEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapGet("/{publicSaleId:guid}/pricing/wheels-and-axles", HandleAsync)
            .WithSummary("Get wheels and axles price")
            .WithDescription("Calculates W&A price by wheel and axle count via iSeries adapter.")
            .WithName("GetWheelsAndAxlesPrice")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<WheelsAndAxlesPriceResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> HandleAsync(
        Guid publicSaleId,
        int numberOfWheels,
        int numberOfAxles,
        ISender sender,
        CancellationToken ct)
    {
        var query = new GetWheelsAndAxlesPriceQuery(publicSaleId, numberOfWheels, numberOfAxles);

        var result = await sender.Send(query, ct);

        return result.Match(
            price => Results.Ok(new WheelsAndAxlesPriceResponse(price)),
            ApiResults.Problem);
    }
}

public sealed record WheelsAndAxlesPriceResponse(decimal WheelsAndAxlesPrice);
