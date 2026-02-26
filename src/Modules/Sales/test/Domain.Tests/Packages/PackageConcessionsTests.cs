using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Credits;
using Xunit;

namespace Modules.Sales.Domain.Tests.Packages;

public sealed class PackageConcessionsTests
{
    [Fact]
    public void Adding_concession_does_not_change_gross_profit()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);
        var gpBefore = package.GrossProfit;

        package.AddLine(CreditLine.CreateConcession(package.Id, 5000m));

        Assert.Equal(gpBefore, package.GrossProfit);
        Assert.Equal(gpBefore, package.CommissionableGrossProfit);
    }

    [Fact]
    public void Removing_concession_does_not_change_gross_profit()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);
        var line = CreditLine.CreateConcession(package.Id, 5000m);
        package.AddLine(line);
        var gpBefore = package.GrossProfit;

        package.RemoveLine(line);

        Assert.Equal(gpBefore, package.GrossProfit);
    }

    [Fact]
    public void Concession_line_appears_in_package_lines()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);

        package.AddLine(CreditLine.CreateConcession(package.Id, 3000m));

        var credit = Assert.Single(package.Lines.OfType<CreditLine>());
        Assert.True(credit.IsConcession);
        Assert.Equal(3000m, credit.SalePrice);
    }

    [Fact]
    public void Concession_estimated_cost_is_zero()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);

        package.AddLine(CreditLine.CreateConcession(package.Id, 2500m));

        var credit = Assert.Single(package.Lines.OfType<CreditLine>());
        Assert.Equal(0m, credit.EstimatedCost);
    }

    [Fact]
    public void Concession_and_down_payment_coexist_without_interference()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);

        package.AddLine(CreditLine.CreateDownPayment(package.Id, 5000m));
        package.AddLine(CreditLine.CreateConcession(package.Id, 2000m));

        var credits = package.Lines.OfType<CreditLine>().ToList();
        Assert.Equal(2, credits.Count);
        Assert.Single(credits, c => c.IsDownPayment);
        Assert.Single(credits, c => c.IsConcession);
    }
}
