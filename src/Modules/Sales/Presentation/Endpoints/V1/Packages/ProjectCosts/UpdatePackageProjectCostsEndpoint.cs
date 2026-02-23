using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Sales.Application.Packages.UpdatePackageProjectCosts;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.Presentation.Endpoints.V1.Packages.ProjectCosts;

internal sealed class UpdatePackageProjectCostsEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPut("/{publicPackageId:guid}/project-costs", HandleAsync)
            .WithSummary("Update package project costs section")
            .WithDescription("Replaces the project costs collection. PUT semantics — always replaces all existing project cost lines.")
            .WithName("UpdatePackageProjectCosts")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<PackageUpdatedResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> HandleAsync(
        Guid publicPackageId,
        ProjectCostLineItem[] items,
        ISender sender,
        CancellationToken ct)
    {
        var command = new UpdatePackageProjectCostsCommand(
            publicPackageId,
            items.Select(i => new UpdateProjectCostItemRequest(
                i.CategoryId,
                i.ItemId,
                i.SalePrice,
                i.EstimatedCost,
                i.RetailSalePrice,
                i.ShouldExcludeFromPricing,
                i.Responsibility)).ToArray());

        var result = await sender.Send(command, ct);

        return result.Match(
            r => Results.Ok(new PackageUpdatedResponse(
                r.GrossProfit,
                r.CommissionableGrossProfit,
                r.MustRecalculateTaxes)),
            ApiResults.Problem);
    }
}

public sealed record ProjectCostLineItem(
    int CategoryId,
    int ItemId,
    decimal SalePrice,
    decimal EstimatedCost,
    decimal RetailSalePrice,
    bool ShouldExcludeFromPricing,
    string? Responsibility = null);
