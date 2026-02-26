using Modules.Sales.Domain.InventoryCache;

namespace Modules.Sales.Domain.Packages.Land;

// Land line — land/lot associated with the home sale. 1:1 per package.
// Excluded from GP — legacy never included land in the gross profit formula.
// Land's financial impact flows through the shadow Land Payoff project cost (Cat 2, Item 1).
public sealed class LandLine : PackageLine<LandDetails>
{
    public override bool ShouldExcludeFromPricing => true;

    // NULL for customer-owned/private/community land
    public int? LandParcelId { get; private set; }
    public LandParcelCache? LandParcel { get; private set; }

    // Severs the link to the inventory product cache.
    // Called in two scenarios:
    //   1. Inventory removes the product from its catalog (RemovedFromInventory event)
    //   2. Another sale claims this land parcel (Flow 2 — TBD trigger)
    // LandDetails JSONB is preserved — the line retains all product specs as manual data.
    public void DetachProduct()
    {
        LandParcelId = null;
        LandParcel = null;
    }

    private LandLine() { }

    public static LandLine Create(
        int packageId,
        decimal salePrice,
        decimal estimatedCost,
        decimal retailSalePrice,
        Responsibility? responsibility,
        LandDetails? details,
        int? landParcelId = null)
    {
        return new LandLine
        {
            PackageId = packageId,
            LineType = PackageLineTypeConstants.Land,
            SalePrice = Math.Round(salePrice, 2),
            EstimatedCost = Math.Round(estimatedCost, 2),
            RetailSalePrice = Math.Round(retailSalePrice, 2),
            Responsibility = responsibility,
            Details = details,
            LandParcelId = landParcelId
        };
    }
}
