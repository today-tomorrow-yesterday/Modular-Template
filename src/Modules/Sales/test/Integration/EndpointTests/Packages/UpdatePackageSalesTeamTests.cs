using System.Net;
using System.Net.Http.Json;
using Modules.Sales.Application.Packages.UpdatePackageSalesTeam;
using Modules.Sales.Domain.Packages.SalesTeam;
using Modules.Sales.EndpointTests.Abstractions;

namespace Modules.Sales.EndpointTests.Packages;

public class UpdatePackageSalesTeamTests(SalesEndpointTestFixture fixture) : SalesEndpointTestBase(fixture)
{
    private const string PrimarySalespersonRole = "Primary Salesperson";
    private const string SecondarySalespersonRole = "Secondary Salesperson";

    private string Endpoint => $"/api/v1/packages/{PackageId}/sales-team";

    [Fact]
    public async Task SalesTeam_TwoMembers()
    {
        // Arrange
        var primarySplitPercentage = 60.0m;
        var secondarySplitPercentage = 40.0m;
        await ArrangeSaleWithHomeAsync();
        var packageBeforeUpdate = await GetPackageAsync();

        // Act
        var response = await Client.PutAsJsonAsync(Endpoint, new[]
        {
            new UpdatePackageSalesTeamMemberRequest(
                SalesEndpointTestFixture.TestAuthorizedUserId1,
                SalesTeamRole.Primary,
                primarySplitPercentage),
            new UpdatePackageSalesTeamMemberRequest(
                SalesEndpointTestFixture.TestAuthorizedUserId2,
                SalesTeamRole.Secondary,
                secondarySplitPercentage)
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);                      // Should have returned 200 OK
        var updatedPackage = await GetPackageAsync();

        Assert.Equal(2, updatedPackage.SalesTeam.Length);                          // Should have two sales team members

        var primary = Assert.Single(updatedPackage.SalesTeam,
            member => member.Role == PrimarySalespersonRole);
        Assert.Equal(primarySplitPercentage, primary.CommissionSplitPercentage);   // Should set 60% split for primary
        Assert.True(primary.ShouldExcludeFromPricing);                             // Should exclude sales team from pricing

        var secondary = Assert.Single(updatedPackage.SalesTeam,
            member => member.Role == SecondarySalespersonRole);
        Assert.Equal(secondarySplitPercentage, secondary.CommissionSplitPercentage); // Should set 40% split for secondary

        Assert.Equal(packageBeforeUpdate.GrossProfit, updatedPackage.GrossProfit); // Should not change gross profit
    }

    [Fact]
    public async Task SalesTeam_SingleMember()
    {
        // Arrange
        var splitPercentage = 100.0m;
        await ArrangeSaleWithHomeAsync();
        var packageBeforeUpdate = await GetPackageAsync();

        // Act
        var response = await Client.PutAsJsonAsync(Endpoint, new[]
        {
            new UpdatePackageSalesTeamMemberRequest(
                SalesEndpointTestFixture.TestAuthorizedUserId1,
                SalesTeamRole.Primary,
                splitPercentage)
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);                      // Should have returned 200 OK
        var updatedPackage = await GetPackageAsync();

        var member = Assert.Single(updatedPackage.SalesTeam);
        Assert.Equal(splitPercentage, member.CommissionSplitPercentage);            // Should set 100% split
        Assert.True(member.ShouldExcludeFromPricing);                              // Should exclude sales team from pricing

        Assert.Equal(packageBeforeUpdate.GrossProfit, updatedPackage.GrossProfit); // Should not change gross profit
    }
}
