using System.Net;
using System.Net.Http.Json;
using Modules.Sales.Domain.Packages.Insurance;
using Modules.Sales.IntegrationTests.Abstractions;
using Modules.Sales.Presentation.Endpoints.V1.Packages.Insurance;

namespace Modules.Sales.IntegrationTests.Packages;

public class UpdatePackageInsuranceTests(SalesIntegrationTestFixture fixture) : SalesIntegrationTestBase(fixture)
{
    private string Endpoint => $"/api/v1/packages/{PackageId}/insurance";

    [Fact]
    public async Task Insurance_HomeFirst()
    {
        // Arrange
        await ArrangeSaleWithHomeAsync();
        var packageBeforeUpdate = await GetPackageAsync();

        // Act
        var response = await Client.PutAsJsonAsync(Endpoint, new UpdatePackageInsuranceRequest(
            InsuranceType: nameof(InsuranceType.HomeFirst),
            CoverageAmount: 300_000m,
            HasFoundationOrMasonry: false,
            InParkOrSubdivision: false,
            IsLandOwnedByCustomer: true,
            IsPremiumFinanced: true,
            CompanyName: "HomeFirst Insurance Co",
            MaxCoverage: 350_000m,
            TotalPremium: 800m));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode); // Should have returned 200 OK
        var updatedPackage = await GetPackageAsync();

        Assert.NotNull(updatedPackage.Insurance); // Should create insurance section
        Assert.Equal(nameof(InsuranceType.HomeFirst), updatedPackage.Insurance.InsuranceType); // Should set insurance type
        Assert.Equal(800m, updatedPackage.Insurance.SalePrice); // Should set SP to total premium

        // GP = packageBeforeUpdate.GP + insuranceSP = before + 800
        Assert.Equal(packageBeforeUpdate.GrossProfit + 800m, updatedPackage.GrossProfit); // Should increase GP by insurance sale price
    }

    [Fact]
    public async Task Insurance_Outside()
    {
        // Arrange
        await ArrangeSaleWithHomeAsync();
        var packageBeforeUpdate = await GetPackageAsync();

        // Act
        var response = await Client.PutAsJsonAsync(Endpoint, new UpdatePackageInsuranceRequest(
            InsuranceType: nameof(InsuranceType.Outside),
            CoverageAmount: 250_000m,
            HasFoundationOrMasonry: false,
            InParkOrSubdivision: false,
            IsLandOwnedByCustomer: false,
            IsPremiumFinanced: false,
            CompanyName: "External Insurance LLC",
            MaxCoverage: 300_000m,
            TotalPremium: 600m));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode); // Should have returned 200 OK
        var updatedPackage = await GetPackageAsync();

        Assert.NotNull(updatedPackage.Insurance); // Should create insurance section
        Assert.Equal(nameof(InsuranceType.Outside), updatedPackage.Insurance.InsuranceType); // Should set insurance type
        Assert.Equal(600m, updatedPackage.Insurance.SalePrice); // Should set SP to total premium

        // GP = packageBeforeUpdate.GP + insuranceSP = before + 600
        Assert.Equal(packageBeforeUpdate.GrossProfit + 600m, updatedPackage.GrossProfit); // Should increase GP by insurance sale price
    }
}
