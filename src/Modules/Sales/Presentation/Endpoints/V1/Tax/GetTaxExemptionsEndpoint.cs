using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Sales.Application.Tax.GetTaxExemptions;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.Presentation.Endpoints.V1.Tax;

internal sealed class GetTaxExemptionsEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapGet("/tax/exemptions", HandleAsync)
            .WithSummary("Get tax exemptions")
            .WithDescription("Returns all active tax exemption codes from CDC reference data.")
            .WithName("GetTaxExemptions")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<IReadOnlyCollection<TaxExemptionResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> HandleAsync(
        ISender sender,
        CancellationToken ct)
    {
        var query = new GetTaxExemptionsQuery();

        var result = await sender.Send(query, ct);

        return result.Match(
            exemptions => Results.Ok(exemptions.Select(e => new TaxExemptionResponse(
                e.ExemptionCode,
                e.Description,
                e.RulesText)).ToList()),
            ApiResults.Problem);
    }
}

public sealed record TaxExemptionResponse(
    int ExemptionCode,
    string? Description,
    string? RulesText);
