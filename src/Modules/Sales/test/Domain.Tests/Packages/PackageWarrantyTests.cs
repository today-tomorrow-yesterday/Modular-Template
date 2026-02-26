using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Warranty;
using Xunit;

namespace Modules.Sales.Domain.Tests.Packages;

public sealed class PackageWarrantyTests
{
    [Fact]
    public void WarrantyDetails_Create_sets_all_properties()
    {
        var details = WarrantyDetails.Create(875.00m, 72.19m);

        Assert.True(details.WarrantySelected);
        Assert.Equal(875.00m, details.WarrantyAmount);
        Assert.Equal(72.19m, details.SalesTaxPremium);
        Assert.NotNull(details.QuotedAt);
    }

    [Fact]
    public void WarrantyLine_Create_sets_pricing_fields()
    {
        var details = WarrantyDetails.Create(875.00m, 72.19m);

        var line = WarrantyLine.Create(
            packageId: 1,
            salePrice: 875.00m,
            estimatedCost: 0m,
            retailSalePrice: 0m,
            shouldExcludeFromPricing: false,
            details: details);

        Assert.Equal(875.00m, line.SalePrice);
        Assert.Equal(0m, line.EstimatedCost);
        Assert.Equal(0m, line.RetailSalePrice);
        Assert.False(line.ShouldExcludeFromPricing);
        Assert.Equal(PackageLineTypeConstants.Warranty, line.LineType);
    }

    [Fact]
    public void WarrantyLine_rounds_sale_price_to_two_decimals()
    {
        var details = WarrantyDetails.Create(875.555m, 72.199m);

        var line = WarrantyLine.Create(
            packageId: 1,
            salePrice: 875.555m,
            estimatedCost: 0m,
            retailSalePrice: 0m,
            shouldExcludeFromPricing: false,
            details: details);

        Assert.Equal(875.56m, line.SalePrice);
    }

    [Fact]
    public void Adding_warranty_line_does_not_affect_gross_profit_when_excluded()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);
        var gpBefore = package.GrossProfit;

        var details = WarrantyDetails.Create(875.00m, 72.19m);
        package.AddLine(WarrantyLine.Create(
            package.Id, salePrice: 875.00m, estimatedCost: 0m,
            retailSalePrice: 0m, shouldExcludeFromPricing: true, details: details));

        Assert.Equal(gpBefore, package.GrossProfit);
    }

    [Fact]
    public void Removing_warranty_line_does_not_affect_gross_profit()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);
        var details = WarrantyDetails.Create(875.00m, 72.19m);
        var line = WarrantyLine.Create(
            package.Id, salePrice: 875.00m, estimatedCost: 0m,
            retailSalePrice: 0m, shouldExcludeFromPricing: true, details: details);
        package.AddLine(line);
        var gpBefore = package.GrossProfit;

        package.RemoveLine(line);

        Assert.Equal(gpBefore, package.GrossProfit);
    }

    [Fact]
    public void Warranty_line_is_1_to_1_per_package()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);

        var details = WarrantyDetails.Create(875.00m, 72.19m);
        package.AddLine(WarrantyLine.Create(
            package.Id, salePrice: 875.00m, estimatedCost: 0m,
            retailSalePrice: 0m, shouldExcludeFromPricing: false, details: details));

        var warranty = Assert.Single(package.Lines.OfType<WarrantyLine>());
        Assert.NotNull(warranty.Details);
        Assert.True(warranty.Details.WarrantySelected);
    }
}
