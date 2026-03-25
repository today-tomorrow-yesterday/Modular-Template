using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Modules.Sales.Application.Packages.UpdatePackageSalesTeam;
using Modules.Sales.Domain.AuthorizedUsersCache;
using Modules.Sales.Domain.Packages.SalesTeam;
using Modules.Sales.Infrastructure.Persistence;
using Modules.Sales.IntegrationTests.Abstractions;
using Rtl.Core.Application.Caching;

namespace Modules.Sales.IntegrationTests.Packages;

public class UpdatePackageSalesTeamTests(SalesTestFactory factory) : SalesIntegrationTestBase(factory)
{
    private string Endpoint => $"/api/v1/packages/{PackageId}/sales-team";

    private async Task SeedAuthorizedUsersAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SalesDbContext>();
        var cacheScope = scope.ServiceProvider.GetRequiredService<ICacheWriteScope>();

        using (cacheScope.AllowWrites())
        {
            db.Set<AuthorizedUserCache>().AddRange(
                new AuthorizedUserCache
                {
                    Id = SalesTestFactory.TestAuthorizedUserId1,
                    RefUserId = SalesTestFactory.TestAuthorizedUserId1,
                    FederatedId = "fed-001",
                    EmployeeNumber = 1001,
                    FirstName = "Alice",
                    LastName = "Sales",
                    DisplayName = "Alice Sales",
                    EmailAddress = "alice@test.com",
                    IsActive = true,
                    IsRetired = false,
                    AuthorizedHomeCenters = [TestHomeCenterNumber],
                    LastSyncedAtUtc = DateTime.UtcNow
                },
                new AuthorizedUserCache
                {
                    Id = SalesTestFactory.TestAuthorizedUserId2,
                    RefUserId = SalesTestFactory.TestAuthorizedUserId2,
                    FederatedId = "fed-002",
                    EmployeeNumber = 1002,
                    FirstName = "Bob",
                    LastName = "Sales",
                    DisplayName = "Bob Sales",
                    EmailAddress = "bob@test.com",
                    IsActive = true,
                    IsRetired = false,
                    AuthorizedHomeCenters = [TestHomeCenterNumber],
                    LastSyncedAtUtc = DateTime.UtcNow
                });
            await db.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task SalesTeam_TwoMembers()
    {
        // Arrange
        await ArrangeSaleWithHomeAsync();
        await SeedAuthorizedUsersAsync();
        var packageBeforeUpdate = await GetPackageAsync();

        // Act
        var response = await Client.PutAsJsonAsync(Endpoint, new[]
        {
            new UpdatePackageSalesTeamMemberRequest(
                SalesTestFactory.TestAuthorizedUserId1,
                SalesTeamRole.Primary,
                60.0m),
            new UpdatePackageSalesTeamMemberRequest(
                SalesTestFactory.TestAuthorizedUserId2,
                SalesTeamRole.Secondary,
                40.0m)
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode); // Should have returned 200 OK
        var updatedPackage = await GetPackageAsync();

        Assert.Equal(2, updatedPackage.SalesTeam.Length); // Should have two sales team members

        var primary = Assert.Single(updatedPackage.SalesTeam,
            member => member.Role == "Primary Salesperson");
        Assert.Equal(60.0m, primary.CommissionSplitPercentage); // Should set 60% split for primary
        Assert.True(primary.ShouldExcludeFromPricing); // Should exclude sales team from pricing

        var secondary = Assert.Single(updatedPackage.SalesTeam,
            member => member.Role == "Secondary Salesperson");
        Assert.Equal(40.0m, secondary.CommissionSplitPercentage); // Should set 40% split for secondary

        Assert.Equal(packageBeforeUpdate.GrossProfit, updatedPackage.GrossProfit); // Should not change gross profit
    }

    [Fact]
    public async Task SalesTeam_SingleMember()
    {
        // Arrange
        await ArrangeSaleWithHomeAsync();
        await SeedAuthorizedUsersAsync();
        var packageBeforeUpdate = await GetPackageAsync();

        // Act
        var response = await Client.PutAsJsonAsync(Endpoint, new[]
        {
            new UpdatePackageSalesTeamMemberRequest(
                SalesTestFactory.TestAuthorizedUserId1,
                SalesTeamRole.Primary,
                100.0m)
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode); // Should have returned 200 OK
        var updatedPackage = await GetPackageAsync();

        Assert.Single(updatedPackage.SalesTeam); // Should have one sales team member
        Assert.Equal(100.0m, updatedPackage.SalesTeam[0].CommissionSplitPercentage); // Should set 100% split
        Assert.True(updatedPackage.SalesTeam[0].ShouldExcludeFromPricing); // Should exclude sales team from pricing

        Assert.Equal(packageBeforeUpdate.GrossProfit, updatedPackage.GrossProfit); // Should not change gross profit
    }
}
