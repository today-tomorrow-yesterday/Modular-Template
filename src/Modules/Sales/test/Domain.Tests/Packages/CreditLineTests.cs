using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Details;
using Modules.Sales.Domain.Packages.Lines;
using Xunit;

namespace Modules.Sales.Domain.Tests.Packages;

public sealed class CreditLineTests
{
    [Fact]
    public void CreateDownPayment_sets_correct_properties()
    {
        var line = CreditLine.CreateDownPayment(packageId: 42, amount: 5000m);

        Assert.Equal(PackageLineTypeConstants.Credit, line.LineType);
        Assert.Equal(5000m, line.SalePrice);
        Assert.Equal(0m, line.EstimatedCost);
        Assert.Equal(0m, line.RetailSalePrice);
        Assert.Equal(Responsibility.Buyer, line.Responsibility);
        Assert.NotNull(line.Details);
        Assert.Equal(CreditType.DownPayment, line.Details!.CreditType);
    }

    [Fact]
    public void CreateDownPayment_rounds_amount_to_two_decimals()
    {
        var line = CreditLine.CreateDownPayment(packageId: 1, amount: 1234.5678m);

        Assert.Equal(1234.57m, line.SalePrice);
    }

    [Fact]
    public void CreateConcession_sets_correct_properties()
    {
        var line = CreditLine.CreateConcession(packageId: 42, amount: 2000m);

        Assert.Equal(PackageLineTypeConstants.Credit, line.LineType);
        Assert.Equal(2000m, line.SalePrice);
        Assert.Equal(0m, line.EstimatedCost); // Seller's cost on Seller Paid Closing Cost project cost, not the credit line
        Assert.Equal(0m, line.RetailSalePrice);
        Assert.Equal(Responsibility.Seller, line.Responsibility);
        Assert.NotNull(line.Details);
        Assert.Equal(CreditType.Concessions, line.Details!.CreditType);
    }

    [Fact]
    public void IsDownPayment_returns_true_for_down_payment()
    {
        var line = CreditLine.CreateDownPayment(packageId: 1, amount: 1000m);

        Assert.True(line.IsDownPayment);
    }

    [Fact]
    public void IsDownPayment_returns_false_for_concession()
    {
        var line = CreditLine.CreateConcession(packageId: 1, amount: 1000m);

        Assert.False(line.IsDownPayment);
    }

    [Fact]
    public void IsConcession_returns_true_for_concession()
    {
        var line = CreditLine.CreateConcession(packageId: 1, amount: 1000m);

        Assert.True(line.IsConcession);
    }

    [Fact]
    public void IsConcession_returns_false_for_down_payment()
    {
        var line = CreditLine.CreateDownPayment(packageId: 1, amount: 1000m);

        Assert.False(line.IsConcession);
    }

    [Fact]
    public void ShouldExcludeFromPricing_is_always_true()
    {
        var downPayment = CreditLine.CreateDownPayment(packageId: 1, amount: 500m);
        var concession = CreditLine.CreateConcession(packageId: 1, amount: 500m);

        Assert.True(downPayment.ShouldExcludeFromPricing);
        Assert.True(concession.ShouldExcludeFromPricing);
    }

    [Fact]
    public void UpdatePricing_updates_sale_price_with_rounding()
    {
        var line = CreditLine.CreateDownPayment(packageId: 1, amount: 1000m);

        line.UpdatePricing(salePrice: 2500.999m, estimatedCost: 0m, retailSalePrice: 0m);

        Assert.Equal(2501.00m, line.SalePrice);
        Assert.Equal(0m, line.EstimatedCost);
        Assert.Equal(0m, line.RetailSalePrice);
    }
}
