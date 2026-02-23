using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Details;
using Modules.Sales.Domain.Packages.Lines;
using Xunit;

namespace Modules.Sales.Domain.Tests.Packages;

public sealed class PackageTradeInTests
{
    [Fact]
    public void Adding_trade_in_does_not_change_gross_profit()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);
        var gpBefore = package.GrossProfit;

        package.AddLine(CreateTradeInLine(package.Id, salePrice: 15000m));

        Assert.Equal(gpBefore, package.GrossProfit);
        Assert.Equal(gpBefore, package.CommissionableGrossProfit);
    }

    [Fact]
    public void Removing_trade_in_does_not_change_gross_profit()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);
        var line = CreateTradeInLine(package.Id, salePrice: 15000m);
        package.AddLine(line);
        var gpBefore = package.GrossProfit;

        package.RemoveLine(line);

        Assert.Equal(gpBefore, package.GrossProfit);
    }

    [Fact]
    public void Trade_in_line_appears_in_package_lines()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);

        package.AddLine(CreateTradeInLine(package.Id, salePrice: 12000m));

        var tradeIn = Assert.Single(package.Lines.OfType<TradeInLine>());
        Assert.Equal(12000m, tradeIn.SalePrice);
        Assert.True(tradeIn.ShouldExcludeFromPricing);
    }

    [Fact]
    public void Multiple_trade_in_lines_allowed_per_package()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);

        package.AddLine(CreateTradeInLine(package.Id, salePrice: 10000m, sortOrder: 0));
        package.AddLine(CreateTradeInLine(package.Id, salePrice: 8000m, sortOrder: 1));
        package.AddLine(CreateTradeInLine(package.Id, salePrice: 5000m, sortOrder: 2));

        Assert.Equal(3, package.Lines.OfType<TradeInLine>().Count());
    }

    [Fact]
    public void RemoveLinesByType_removes_all_trade_in_lines()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);
        package.AddLine(CreateTradeInLine(package.Id, salePrice: 10000m, sortOrder: 0));
        package.AddLine(CreateTradeInLine(package.Id, salePrice: 8000m, sortOrder: 1));

        package.RemoveLinesByType(PackageLineTypeConstants.TradeIn);

        Assert.Empty(package.Lines.OfType<TradeInLine>());
    }

    [Fact]
    public void RemoveLinesByType_does_not_affect_other_line_types()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);
        package.AddLine(CreditLine.CreateDownPayment(package.Id, 5000m));
        package.AddLine(CreateTradeInLine(package.Id, salePrice: 10000m));

        package.RemoveLinesByType(PackageLineTypeConstants.TradeIn);

        Assert.Empty(package.Lines.OfType<TradeInLine>());
        Assert.Single(package.Lines.OfType<CreditLine>());
    }

    [Fact]
    public void TradeInDetails_Create_sets_all_properties()
    {
        var details = TradeInDetails.Create(
            tradeType: "Single Wide",
            year: 2020,
            make: "Clayton",
            model: "TruMH",
            tradeAllowance: 25000m,
            payoffAmount: 10000m,
            bookInAmount: 18000m,
            floorWidth: 16m,
            floorLength: 76m);

        Assert.Equal("Single Wide", details.TradeType);
        Assert.Equal(2020, details.Year);
        Assert.Equal("Clayton", details.Make);
        Assert.Equal("TruMH", details.Model);
        Assert.Equal(25000m, details.TradeAllowance);
        Assert.Equal(10000m, details.PayoffAmount);
        Assert.Equal(18000m, details.BookInAmount);
        Assert.Equal(16m, details.FloorWidth);
        Assert.Equal(76m, details.FloorLength);
    }

    [Fact]
    public void TradeInDetails_Create_rounds_decimal_values()
    {
        var details = TradeInDetails.Create(
            tradeType: "Double Wide",
            year: 2019,
            make: "Clayton",
            model: "iHouse",
            tradeAllowance: 25000.555m,
            payoffAmount: 10000.444m,
            bookInAmount: 18000.999m);

        Assert.Equal(25000.56m, details.TradeAllowance);
        Assert.Equal(10000.44m, details.PayoffAmount);
        Assert.Equal(18001.00m, details.BookInAmount);
    }

    [Fact]
    public void TradeInDetails_Create_optional_floor_dimensions_default_to_null()
    {
        var details = TradeInDetails.Create(
            tradeType: "Single Wide",
            year: 2021,
            make: "Clayton",
            model: "TruMH",
            tradeAllowance: 20000m,
            payoffAmount: 5000m,
            bookInAmount: 15000m);

        Assert.Null(details.FloorWidth);
        Assert.Null(details.FloorLength);
    }

    private static TradeInLine CreateTradeInLine(int packageId, decimal salePrice, int sortOrder = 0)
    {
        var details = TradeInDetails.Create(
            tradeType: "Single Wide",
            year: 2020,
            make: "Clayton",
            model: "TruMH",
            tradeAllowance: salePrice,
            payoffAmount: 0m,
            bookInAmount: 0m);

        return TradeInLine.Create(
            packageId: packageId,
            salePrice: salePrice,
            estimatedCost: 0m,
            retailSalePrice: 0m,
            responsibility: Responsibility.Buyer,
            details: details,
            sortOrder: sortOrder);
    }
}
