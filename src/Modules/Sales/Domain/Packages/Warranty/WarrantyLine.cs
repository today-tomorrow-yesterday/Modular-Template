namespace Modules.Sales.Domain.Packages.Warranty;

// Warranty line — home buyer protection plan product. 1:1 per package.
// ShouldExcludeFromPricing is configurable.
public sealed class WarrantyLine : PackageLine<WarrantyDetails>
{
    private WarrantyLine() { }

    public static WarrantyLine Create(
        int packageId,
        decimal salePrice,
        decimal estimatedCost,
        decimal retailSalePrice,
        bool shouldExcludeFromPricing,
        WarrantyDetails? details)
    {
        return new WarrantyLine
        {
            PackageId = packageId,
            LineType = PackageLineTypeConstants.Warranty,
            SalePrice = Math.Round(salePrice, 2),
            EstimatedCost = Math.Round(estimatedCost, 2),
            RetailSalePrice = Math.Round(retailSalePrice, 2),
            Responsibility = Packages.Responsibility.Seller,
            ShouldExcludeFromPricing = shouldExcludeFromPricing,
            Details = details
        };
    }
}
