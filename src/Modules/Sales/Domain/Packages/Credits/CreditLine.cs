namespace Modules.Sales.Domain.Packages.Credits;

// Credit line — concessions or down payments. 1:many per package.
// ShouldExcludeFromPricing always true — credits reduce total, not a price component.
// Subtypes distinguished by CreditType enum in JSONB.
public sealed class CreditLine : PackageLine<CreditDetails>
{
    public override bool ShouldExcludeFromPricing => true;

    private CreditLine() { }

    public static CreditLine CreateDownPayment(int packageId, decimal amount)
    {
        return new CreditLine
        {
            PackageId = packageId,
            LineType = PackageLineTypeConstants.Credit,
            SalePrice = Math.Round(amount, 2),
            EstimatedCost = 0m,
            RetailSalePrice = 0m,
            Responsibility = Packages.Responsibility.Buyer,
            Details = CreditDetails.Create(CreditType.DownPayment)
        };
    }

    public static CreditLine CreateConcession(int packageId, decimal amount)
    {
        return new CreditLine
        {
            PackageId = packageId,
            LineType = PackageLineTypeConstants.Credit,
            SalePrice = Math.Round(amount, 2),
            EstimatedCost = 0m, // Seller's cost captured via Seller Paid Closing Cost project cost (Cat 14/Item 1)
            RetailSalePrice = 0m,
            Responsibility = Packages.Responsibility.Seller,
            Details = CreditDetails.Create(CreditType.Concessions)
        };
    }

    public bool IsDownPayment => Details?.CreditType == CreditType.DownPayment;

    public bool IsConcession => Details?.CreditType == CreditType.Concessions;
}
