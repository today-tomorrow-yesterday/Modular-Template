namespace Modules.Sales.Domain.Packages.ProjectCosts;

/// <summary>
/// Well-known project cost category numbers (iSeries CATID via cdc.project_cost_category).
/// </summary>
public static class ProjectCostCategories
{
    public const int WheelsAndAxles = 1;
    public const int LandPayoff = 2;
    public const int Land = 3;
    public const int UseTax = 9;
    public const int TradeOverAllowance = 10;
    public const int Refurbishment = 11;
    public const int RepoCosts = 12;
    public const int MiscellaneousTax = 13;
    public const int SellerPaidClosingCost = 14;
    public const int Decorating = 15;
}
