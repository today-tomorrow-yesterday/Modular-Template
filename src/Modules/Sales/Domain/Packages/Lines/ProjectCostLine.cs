using Modules.Sales.Domain.Packages.Details;

namespace Modules.Sales.Domain.Packages.Lines;

// Project cost line — additional costs (W&A, land payoff, trade-over-allowance, etc.). 1:many per package.
// ShouldExcludeFromPricing is configurable — land payoff (Cat 2, Item 1) is excluded, others are not.
public sealed class ProjectCostLine : PackageLine<ProjectCostDetails>
{
    private ProjectCostLine() { }

    public static ProjectCostLine Create(
        int packageId,
        decimal salePrice,
        decimal estimatedCost,
        decimal retailSalePrice,
        Responsibility? responsibility,
        bool shouldExcludeFromPricing,
        ProjectCostDetails? details,
        int sortOrder = 0)
    {
        return new ProjectCostLine
        {
            PackageId = packageId,
            LineType = PackageLineTypeConstants.ProjectCost,
            SalePrice = Math.Round(salePrice, 2),
            EstimatedCost = Math.Round(estimatedCost, 2),
            RetailSalePrice = Math.Round(retailSalePrice, 2),
            Responsibility = responsibility,
            ShouldExcludeFromPricing = shouldExcludeFromPricing,
            Details = details,
            SortOrder = sortOrder
        };
    }
}
