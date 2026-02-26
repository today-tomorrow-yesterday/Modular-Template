namespace Modules.Sales.Domain.Packages;

/// <summary>
/// Well-known project cost item numbers (iSeries ITEMID via cdc.project_cost_item).
/// Item numbers are only unique within a category — always pair with the category from
/// <see cref="ProjectCostCategories"/>.
/// </summary>
public static class ProjectCostItems
{
    // Category 1: Wheels & Axles
    public const int WaRental = 28;
    public const int WaPurchase = 29;

    // Category 2: Land Payoff
    public const int LandPayoff = 1;

    // Category 9: Use Tax
    public const int UseTax = 21;

    // Category 10: Trade Over Allowance
    public const int TradeOverAllowance = 9;

    // Category 11: Refurbishment
    public const int Cleaning = 1;
    public const int RepairRefurb = 2;
    public const int RefurbParts = 3;
    public const int Drapes = 4;

    // Category 13: Miscellaneous Tax
    public const int TaxUndercollection = 98;

    // Category 14: Seller Paid Closing Cost
    public const int SellerPaidClosingCost = 1;

    // Category 15: Decorating
    public const int DecoratingDrapes = 4;
}
