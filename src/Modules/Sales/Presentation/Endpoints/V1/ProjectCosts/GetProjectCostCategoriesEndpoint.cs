using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Sales.Application.ProjectCosts.GetProjectCostCategories;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.Presentation.Endpoints.V1.ProjectCosts;

internal sealed class GetProjectCostCategoriesEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapGet("/project-costs", HandleAsync)
            .WithSummary("Get project cost categories")
            .WithDescription("Returns project cost categories, items, and state matrix from CDC reference data.")
            .WithName("GetProjectCostCategories")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<ApiEnvelope<ProjectCostCategoriesResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> HandleAsync(
        ISender sender,
        CancellationToken ct)
    {
        var query = new GetProjectCostCategoriesQuery();

        var result = await sender.Send(query, ct);

        return result.Match(
            r => ApiResponse.Ok(new ProjectCostCategoriesResponse(
                Categories: r.Categories.Select(c => new ProjectCostCategoryResponse(
                    c.CategoryNumber,
                    c.Description,
                    c.IsCreditConsideration,
                    c.IsLandDot,
                    c.RestrictFha,
                    c.RestrictCss,
                    c.DisplayForCash,
                    Items: c.Items.Select(i => new ProjectCostItemResponse(
                        i.ItemNumber,
                        i.Description,
                        i.IsActive,
                        i.IsFeeItem,
                        i.IsFhaRestricted,
                        i.IsCssRestricted,
                        i.IsDisplayForCash,
                        i.IsRestrictOptionPrice,
                        i.IsRestrictCssCost,
                        i.IsHopeRefundsIncluded,
                        i.ProfitPercentage)).ToList())).ToList(),
                StateMatrix: r.StateMatrix.Select(m => new ProjectCostStateMatrixResponse(
                    m.CategoryNumber,
                    m.ItemNumber,
                    m.HomeType,
                    m.StateCode,
                    m.TaxBasisManufactured,
                    m.TaxBasisModularOn,
                    m.TaxBasisModularOff,
                    m.IsInsurable)).ToList())),
            ApiResponse.Problem);
    }
}

public sealed record ProjectCostCategoriesResponse(
    IReadOnlyCollection<ProjectCostCategoryResponse> Categories,
    IReadOnlyCollection<ProjectCostStateMatrixResponse> StateMatrix);

public sealed record ProjectCostCategoryResponse(
    int CategoryNumber,
    string Description,
    bool IsCreditConsideration,
    bool IsLandDot,
    bool RestrictFha,
    bool RestrictCss,
    bool DisplayForCash,
    IReadOnlyCollection<ProjectCostItemResponse> Items);

public sealed record ProjectCostItemResponse(
    int ItemNumber,
    string Description,
    bool IsActive,
    bool IsFeeItem,
    bool IsFhaRestricted,
    bool IsCssRestricted,
    bool IsDisplayForCash,
    bool IsRestrictOptionPrice,
    bool IsRestrictCssCost,
    bool IsHopeRefundsIncluded,
    decimal? ProfitPercentage);

public sealed record ProjectCostStateMatrixResponse(
    int CategoryNumber,
    int ItemNumber,
    string HomeType,
    string StateCode,
    decimal TaxBasisManufactured,
    decimal TaxBasisModularOn,
    decimal TaxBasisModularOff,
    bool IsInsurable);
