namespace Modules.Sales.Domain.Packages;

// Discriminator values for PackageLine TPH.
// Used as LineType column values and for type filtering in queries.
public static class PackageLineTypeConstants
{
    public const string Home = "Home";
    public const string Land = "Land";
    public const string Tax = "Tax";
    public const string Insurance = "Insurance";
    public const string Warranty = "Warranty";
    public const string TradeIn = "Trade In";
    public const string SalesTeam = "Sales Team";
    public const string ProjectCost = "Project Cost";
    public const string Credit = "Credit";
    public const string Discount = "Discount";
    public const string Fee = "Fee";

    // 1:1 types — at most one per package
    public static readonly string[] OneToOneTypes =
    [
        Home, Land, Tax, Warranty, SalesTeam
    ];

    // 1:many types — multiple allowed per package
    public static readonly string[] OneToManyTypes =
    [
        Insurance, TradeIn, ProjectCost, Credit, Discount, Fee
    ];

    public static readonly string[] ValidTypes =
    [
        Home, Land, Tax, Insurance, Warranty,
        TradeIn, SalesTeam, ProjectCost, Credit,
        Discount, Fee
    ];
}
