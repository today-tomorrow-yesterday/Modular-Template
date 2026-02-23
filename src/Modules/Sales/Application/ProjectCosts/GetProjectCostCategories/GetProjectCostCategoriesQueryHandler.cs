using Modules.Sales.Domain.Cdc;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.ProjectCosts.GetProjectCostCategories;

// Flow: GET /api/v1/sales/project-costs →
//   query CDC project cost categories + items (WHERE master_dealer = 29) + state matrix
internal sealed class GetProjectCostCategoriesQueryHandler(ICdcProjectCostQueries cdcProjectCostQueries)
    : IQueryHandler<GetProjectCostCategoriesQuery, ProjectCostCategoriesResult>
{
    public async Task<Result<ProjectCostCategoriesResult>> Handle(
        GetProjectCostCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var categories = await cdcProjectCostQueries.GetCategoriesWithItemsAsync(cancellationToken);
        var stateMatrix = await cdcProjectCostQueries.GetStateMatrixAsync(cancellationToken);

        var categoryResults = categories.Select(c => new ProjectCostCategoryResult(
            c.CategoryNumber,
            c.Description,
            c.IsCreditConsideration,
            c.IsLandDot,
            c.RestrictFha,
            c.RestrictCss,
            c.DisplayForCash,
            Items: c.Items.Select(i => new ProjectCostItemResult(
                i.ItemNumber,
                i.Description,
                IsActive: string.Equals(i.Status, "A", StringComparison.OrdinalIgnoreCase),
                i.IsFeeItem,
                i.IsFhaRestricted,
                i.IsCssRestricted,
                i.IsDisplayForCash,
                i.IsRestrictOptionPrice,
                i.IsRestrictCssCost,
                i.IsHopeRefundsIncluded,
                i.ProfitPercentage)).ToList())).ToList();

        var matrixResults = stateMatrix.Select(m => new ProjectCostStateMatrixResult(
            m.CategoryId,
            m.CategoryItemId,
            m.HomeType,
            m.StateCode,
            m.TaxBasisManufactured,
            m.TaxBasisModularOn,
            m.TaxBasisModularOff,
            m.IsInsurable)).ToList();

        return new ProjectCostCategoriesResult(categoryResults, matrixResults);
    }
}
