using System.Net;
using System.Net.Http.Json;
using Modules.Sales.Application.Packages.UpdatePackageSalesTeam;
using Modules.Sales.Domain.Packages.SalesTeam;
using Modules.Sales.IntegrationTests.Abstractions;

namespace Modules.Sales.IntegrationTests.Packages;

public class UpdatePackageSalesTeamTests(SalesIntegrationTestFixture fixture) : SalesIntegrationTestBase(fixture)
{
    private const string PrimarySalespersonRole = "Primary Salesperson";
    private const string SecondarySalespersonRole = "Secondary Salesperson";

    private string Endpoint => $"/api/v1/packages/{PackageId}/sales-team";

    [Fact]
    public async Task SalesTeam_TwoMembers()
    {
        // Arrange
        await ArrangeSaleWithHomeAsync();
        var packageBeforeUpdate = await GetPackageAsync();

        // Act
        var response = await Client.PutAsJsonAsync(Endpoint, new[]
        {
            new UpdatePackageSalesTeamMemberRequest(
                SalesIntegrationTestFixture.TestAuthorizedUserId1,
                SalesTeamRole.Primary,
                60.0m),
            new UpdatePackageSalesTeamMemberRequest(
                SalesIntegrationTestFixture.TestAuthorizedUserId2,
                SalesTeamRole.Secondary,
                40.0m)
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode); // Should have returned 200 OK
        var updatedPackage = await GetPackageAsync();

        Assert.Equal(2, updatedPackage.SalesTeam.Length); // Should have two sales team members

        var primary = Assert.Single(updatedPackage.SalesTeam,
            member => member.Role == PrimarySalespersonRole);
        Assert.Equal(60.0m, primary.CommissionSplitPercentage); // Should set 60% split for primary
        Assert.True(primary.ShouldExcludeFromPricing); // Should exclude sales team from pricing

        var secondary = Assert.Single(updatedPackage.SalesTeam,
            member => member.Role == SecondarySalespersonRole);
        Assert.Equal(40.0m, secondary.CommissionSplitPercentage); // Should set 40% split for secondary

        Assert.Equal(packageBeforeUpdate.GrossProfit, updatedPackage.GrossProfit); // Should not change gross profit
    }

    [Fact]
    public async Task SalesTeam_SingleMember()
    {
        // Arrange
        await ArrangeSaleWithHomeAsync();
        var packageBeforeUpdate = await GetPackageAsync();

        // Act
        var response = await Client.PutAsJsonAsync(Endpoint, new[]
        {
            new UpdatePackageSalesTeamMemberRequest(
                SalesIntegrationTestFixture.TestAuthorizedUserId1,
                SalesTeamRole.Primary,
                100.0m)
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode); // Should have returned 200 OK
        var updatedPackage = await GetPackageAsync();

        var member = Assert.Single(updatedPackage.SalesTeam);
        Assert.Equal(100.0m, member.CommissionSplitPercentage); // Should set 100% split
        Assert.True(member.ShouldExcludeFromPricing); // Should exclude sales team from pricing

        Assert.Equal(packageBeforeUpdate.GrossProfit, updatedPackage.GrossProfit); // Should not change gross profit
    }
}
