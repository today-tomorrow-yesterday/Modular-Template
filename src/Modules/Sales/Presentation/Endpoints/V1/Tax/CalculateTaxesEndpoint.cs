using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Sales.Application.Tax.CalculateTaxes;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.Presentation.Endpoints.V1.Tax;

internal sealed class CalculateTaxesEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPost("/{publicPackageId:guid}/tax", HandleAsync)
            .WithSummary("Calculate taxes for a package")
            .WithDescription("Executes iSeries 4-step calculation sequence. Tax config must be saved first via PUT.")
            .WithName("CalculateTaxes")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<TaxCalculationResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> HandleAsync(
        Guid publicPackageId,
        ISender sender,
        CancellationToken ct)
    {
        var command = new CalculateTaxesCommand(publicPackageId);

        var result = await sender.Send(command, ct);

        return result.Match(
            r => Results.Ok(new TaxCalculationResponse(
                r.GrossProfit,
                r.CommissionableGrossProfit,
                r.MustRecalculateTaxes,
                r.TaxSalePrice,
                r.TaxItems.Select(t => new TaxItemResponse(t.Name, t.IsOverridden, t.CalculatedAmount, t.ChargedAmount)).ToList(),
                r.Errors)),
            ApiResults.Problem);
    }
}

public sealed record TaxCalculationResponse(
    decimal GrossProfit,
    decimal CommissionableGrossProfit,
    bool MustRecalculateTaxes,
    decimal SalePrice,
    IReadOnlyCollection<TaxItemResponse> TaxItems,
    IReadOnlyCollection<string> Errors);

public sealed record TaxItemResponse(
    string Name,
    bool IsOverridden,
    decimal? CalculatedAmount,
    decimal? ChargedAmount);
