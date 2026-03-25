using System.Net;
using System.Net.Http.Json;
using Modules.Sales.IntegrationTests.Abstractions;
using Modules.Sales.Presentation.Endpoints.V1.Packages.DownPayment;

namespace Modules.Sales.IntegrationTests.Packages;

public class UpdatePackageDownPaymentTests(SalesIntegrationTestFixture fixture) : SalesIntegrationTestBase(fixture)
{
    private string Endpoint => $"/api/v1/packages/{PackageId}/down-payment";

    [Fact]
    public async Task DownPayment_WithAmount()
    {
        // Arrange
        await ArrangeSaleWithHomeAsync();
        var packageBeforeUpdate = await GetPackageAsync();

        // Act
        var response = await Client.PutAsJsonAsync(Endpoint, new UpdatePackageDownPaymentRequest(Amount: 5_000m));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode); // Should have returned 200 OK
        var updatedPackage = await GetPackageAsync();

        Assert.NotNull(updatedPackage.DownPayment); // Should create down payment line
        Assert.Equal(5_000m, updatedPackage.DownPayment.SalePrice); // Should set SP to down payment amount
        Assert.True(updatedPackage.DownPayment.ShouldExcludeFromPricing); // Should exclude down payment from pricing

        Assert.Equal(packageBeforeUpdate.GrossProfit, updatedPackage.GrossProfit); // Should not change gross profit
    }

    [Fact]
    public async Task DownPayment_ZeroAmount()
    {
        // Arrange
        await ArrangeSaleWithHomeAsync();
        var packageBeforeUpdate = await GetPackageAsync();

        // Act
        var response = await Client.PutAsJsonAsync(Endpoint, new UpdatePackageDownPaymentRequest(Amount: 0m));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode); // Should have returned 200 OK
        var updatedPackage = await GetPackageAsync();

        Assert.Null(updatedPackage.DownPayment); // Should not create down payment line for zero amount
        Assert.Equal(packageBeforeUpdate.GrossProfit, updatedPackage.GrossProfit); // Should not change gross profit
    }

    [Fact]
    public async Task DownPayment_AmountUpdated()
    {
        // Arrange
        await ArrangeSaleWithHomeAsync();
        var packageBeforeUpdate = await GetPackageAsync();

        // Act — first 5000, then update to 7000
        var response1 = await Client.PutAsJsonAsync(Endpoint, new UpdatePackageDownPaymentRequest(Amount: 5_000m));
        var response2 = await Client.PutAsJsonAsync(Endpoint, new UpdatePackageDownPaymentRequest(Amount: 7_000m));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode); // Should have returned 200 OK
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode); // Should have returned 200 OK
        var updatedPackage = await GetPackageAsync();

        Assert.NotNull(updatedPackage.DownPayment); // Should persist updated down payment line
        Assert.Equal(7_000m, updatedPackage.DownPayment.SalePrice); // Should update SP to new amount
        Assert.True(updatedPackage.DownPayment.ShouldExcludeFromPricing); // Should exclude down payment from pricing

        Assert.Equal(packageBeforeUpdate.GrossProfit, updatedPackage.GrossProfit); // Should not change gross profit
    }
}
