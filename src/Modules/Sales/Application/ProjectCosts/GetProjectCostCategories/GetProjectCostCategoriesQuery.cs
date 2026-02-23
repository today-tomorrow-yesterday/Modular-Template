using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.ProjectCosts.GetProjectCostCategories;

public sealed record GetProjectCostCategoriesQuery : IQuery<ProjectCostCategoriesResult>;

public sealed record ProjectCostCategoriesResult(
    IReadOnlyCollection<ProjectCostCategoryResult> Categories,
    IReadOnlyCollection<ProjectCostStateMatrixResult> StateMatrix);

public sealed record ProjectCostCategoryResult(
    int CategoryNumber,
    string Description,
    bool IsCreditConsideration,
    bool IsLandDot,
    bool RestrictFha,
    bool RestrictCss,
    bool DisplayForCash,
    IReadOnlyCollection<ProjectCostItemResult> Items);

public sealed record ProjectCostItemResult(
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

public sealed record ProjectCostStateMatrixResult(
    int CategoryNumber,
    int ItemNumber,
    string HomeType,
    string StateCode,
    decimal TaxBasisManufactured,
    decimal TaxBasisModularOn,
    decimal TaxBasisModularOff,
    bool IsInsurable);
