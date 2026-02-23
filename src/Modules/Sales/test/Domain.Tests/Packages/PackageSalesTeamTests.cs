using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Details;
using Modules.Sales.Domain.Packages.Lines;
using Xunit;

namespace Modules.Sales.Domain.Tests.Packages;

public sealed class PackageSalesTeamTests
{
    [Fact]
    public void SalesTeamMember_Create_sets_properties_correctly()
    {
        var member = SalesTeamMember.Create(42, SalesTeamRole.Primary, 60m);

        Assert.Equal(42, member.AuthorizedUserId);
        Assert.Equal(SalesTeamRole.Primary, member.Role);
        Assert.Equal(60m, member.CommissionSplitPercentage);
        Assert.Equal(0m, member.CommissionAmount);
    }

    [Fact]
    public void SalesTeamMember_Create_with_null_split_percentage()
    {
        var member = SalesTeamMember.Create(10, SalesTeamRole.Secondary, null);

        Assert.Equal(10, member.AuthorizedUserId);
        Assert.Null(member.CommissionSplitPercentage);
        Assert.Equal(0m, member.CommissionAmount);
    }

    [Fact]
    public void SalesTeamDetails_Create_with_multiple_members()
    {
        var members = new List<SalesTeamMember>
        {
            SalesTeamMember.Create(1, SalesTeamRole.Primary, 60m),
            SalesTeamMember.Create(2, SalesTeamRole.Secondary, 40m)
        };

        var details = SalesTeamDetails.Create(members);

        Assert.Equal(2, details.SalesTeamMembers.Count);
        Assert.Equal(1, details.SchemaVersion);
    }

    [Fact]
    public void SalesTeamDetails_Create_with_empty_list()
    {
        var details = SalesTeamDetails.Create([]);

        Assert.Empty(details.SalesTeamMembers);
    }

    [Fact]
    public void Adding_sales_team_line_does_not_change_gross_profit()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);
        var gpBefore = package.GrossProfit;

        var details = SalesTeamDetails.Create(
        [
            SalesTeamMember.Create(1, SalesTeamRole.Primary, 100m)
        ]);
        package.AddLine(SalesTeamLine.Create(package.Id, details));

        Assert.Equal(gpBefore, package.GrossProfit);
        Assert.Equal(gpBefore, package.CommissionableGrossProfit);
    }

    [Fact]
    public void Removing_sales_team_line_does_not_change_gross_profit()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);
        var details = SalesTeamDetails.Create(
        [
            SalesTeamMember.Create(1, SalesTeamRole.Primary, 100m)
        ]);
        var line = SalesTeamLine.Create(package.Id, details);
        package.AddLine(line);
        var gpBefore = package.GrossProfit;

        package.RemoveLine(line);

        Assert.Equal(gpBefore, package.GrossProfit);
    }

    [Fact]
    public void Sales_team_line_has_zero_pricing_values()
    {
        var details = SalesTeamDetails.Create(
        [
            SalesTeamMember.Create(1, SalesTeamRole.Primary, 100m)
        ]);

        var line = SalesTeamLine.Create(packageId: 5, details);

        Assert.Equal(0m, line.SalePrice);
        Assert.Equal(0m, line.EstimatedCost);
        Assert.Equal(0m, line.RetailSalePrice);
        Assert.Equal(PackageLineTypeConstants.SalesTeam, line.LineType);
    }

    [Fact]
    public void Sales_team_line_appears_in_package_lines()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);
        var details = SalesTeamDetails.Create(
        [
            SalesTeamMember.Create(1, SalesTeamRole.Primary, 60m),
            SalesTeamMember.Create(2, SalesTeamRole.Secondary, 40m)
        ]);

        package.AddLine(SalesTeamLine.Create(package.Id, details));

        var salesTeam = Assert.Single(package.Lines.OfType<SalesTeamLine>());
        Assert.NotNull(salesTeam.Details);
        Assert.Equal(2, salesTeam.Details!.SalesTeamMembers.Count);
    }
}
