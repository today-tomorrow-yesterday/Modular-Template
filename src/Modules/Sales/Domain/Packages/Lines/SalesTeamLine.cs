using Modules.Sales.Domain.Packages.Details;

namespace Modules.Sales.Domain.Packages.Lines;

// Sales team line — commission assignments. 1:1 per package.
// ShouldExcludeFromPricing is explicitly true — metadata-only line with all-zero prices.
public sealed class SalesTeamLine : PackageLine<SalesTeamDetails>
{
    public override bool ShouldExcludeFromPricing => true;

    private SalesTeamLine() { }

    public static SalesTeamLine Create(
        int packageId,
        SalesTeamDetails? details)
    {
        return new SalesTeamLine
        {
            PackageId = packageId,
            LineType = PackageLineTypeConstants.SalesTeam,
            SalePrice = 0m,
            EstimatedCost = 0m,
            RetailSalePrice = 0m,
            Details = details
        };
    }
}
