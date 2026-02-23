using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Sales.Application.Pricing.GetHomeMultipliers;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.Presentation.Endpoints.V1.Pricing;

internal sealed class GetHomeMultipliersEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapGet("/{publicSaleId:guid}/pricing/home-multipliers", HandleAsync)
            .WithSummary("Get home multipliers for a sale")
            .WithDescription("Returns active home multipliers from CDC reference data. Handler derives stateCode from sale context.")
            .WithName("GetHomeMultipliers")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<HomeMultipliersResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> HandleAsync(
        Guid publicSaleId,
        DateOnly? effectiveDate,
        ISender sender,
        CancellationToken ct)
    {
        var query = new GetHomeMultipliersQuery(publicSaleId, effectiveDate);

        var result = await sender.Send(query, ct);

        return result.Match(
            m => Results.Ok(new HomeMultipliersResponse(
                m.EffectiveDate.ToString("yyyy-MM-dd"),
                m.BaseHomeMultiplier,
                m.UpgradesMultiplier,
                m.FreightMultiplier,
                m.WheelsAxlesMultiplier,
                m.DuesMultiplier)),
            ApiResults.Problem);
    }
}

public sealed record HomeMultipliersResponse(
    string EffectiveDate,
    decimal BaseHomeMultiplier,
    decimal UpgradesMultiplier,
    decimal FreightMultiplier,
    decimal WheelsAxlesMultiplier,
    decimal DuesMultiplier);
