using Modules.Sales.Domain.Packages.Details;

namespace Modules.Sales.Domain.Packages.Lines;

// Insurance line — HomeFirst or other insurance products. 1:many per package.
// ShouldExcludeFromPricing is configurable per insurance product.
public sealed class InsuranceLine : PackageLine<InsuranceDetails>
{
    private InsuranceLine() { }

    public static InsuranceLine Create(
        int packageId,
        decimal salePrice,
        decimal estimatedCost,
        decimal retailSalePrice,
        Responsibility? responsibility,
        bool shouldExcludeFromPricing,
        InsuranceDetails? details,
        int sortOrder = 0)
    {
        return new InsuranceLine
        {
            PackageId = packageId,
            LineType = PackageLineTypeConstants.Insurance,
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
