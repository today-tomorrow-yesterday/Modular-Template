using Modules.Sales.Domain.InventoryCache;
using Modules.Sales.Domain.Packages.Details;

namespace Modules.Sales.Domain.Packages.Lines;

// Home line — the manufactured home being sold. 1:1 per package.
// ShouldExcludeFromPricing always false — home always participates in pricing.
public sealed class HomeLine : PackageLine<HomeDetails>
{
    public override bool ShouldExcludeFromPricing => false;

    // NULL for manual/custom homes
    public int? OnLotHomeId { get; private set; }
    public OnLotHomeCache? OnLotHome { get; private set; }

    // Severs the link to the inventory product cache.
    // Called in two scenarios:
    //   1. Inventory removes the product from its catalog (RemovedFromInventory event)
    //   2. Another sale claims this product (Flow 2 — TBD trigger)
    // HomeDetails JSONB is preserved — the line retains all product specs as manual data.
    public void DetachProduct()
    {
        OnLotHomeId = null;
        OnLotHome = null;
    }

    private HomeLine() { }

    public static HomeLine Create(
        int packageId,
        decimal salePrice,
        decimal estimatedCost,
        decimal retailSalePrice,
        Responsibility? responsibility,
        HomeDetails? details,
        int? onLotHomeId = null)
    {
        return new HomeLine
        {
            PackageId = packageId,
            LineType = PackageLineTypeConstants.Home,
            SalePrice = Math.Round(salePrice, 2),
            EstimatedCost = Math.Round(estimatedCost, 2),
            RetailSalePrice = Math.Round(retailSalePrice, 2),
            Responsibility = responsibility,
            Details = details,
            OnLotHomeId = onLotHomeId
        };
    }
}
