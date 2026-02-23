using Modules.Sales.Domain.Packages.Details;

namespace Modules.Sales.Domain.Packages.Lines;

// Trade-in line — home being traded in. 1:many per package.
// ShouldExcludeFromPricing always true — trade-in value is a credit, not a price component.
public sealed class TradeInLine : PackageLine<TradeInDetails>
{
    public override bool ShouldExcludeFromPricing => true;

    private TradeInLine() { }

    public static TradeInLine Create(
        int packageId,
        decimal salePrice,
        decimal estimatedCost,
        decimal retailSalePrice,
        Responsibility? responsibility,
        TradeInDetails? details,
        int sortOrder = 0)
    {
        return new TradeInLine
        {
            PackageId = packageId,
            LineType = PackageLineTypeConstants.TradeIn,
            SalePrice = Math.Round(salePrice, 2),
            EstimatedCost = Math.Round(estimatedCost, 2),
            RetailSalePrice = Math.Round(retailSalePrice, 2),
            Responsibility = responsibility,
            Details = details,
            SortOrder = sortOrder
        };
    }
}
