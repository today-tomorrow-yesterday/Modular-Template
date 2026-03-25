using System.Net.Http.Json;
using Modules.Sales.Application.Packages.UpdatePackageSalesTeam;
using Modules.Sales.Domain.Packages.SalesTeam;
using Modules.Sales.EndpointTests.Abstractions;
using Modules.Sales.Presentation.Endpoints.V1.Commission;
using Modules.Sales.Presentation.Endpoints.V1.Tax;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.EndpointTests.Commission;

public class CalculateCommissionTests(SalesEndpointTestFixture fixture) : SalesEndpointTestBase(fixture)
{
    private string Endpoint => $"/api/v1/packages/{PackageId}/commission";

    [Fact]
    public async Task Should_CalculateCommission_WhenPackageHasRequiredData()
    {
        // Arrange — sale + delivery + home + sales team + funding cache
        await ArrangeSaleWithSalesTeamAsync();
        await Fixture.SeedFundingRequestCacheAsync(SaleId, PackageId);

        // Act
        var response = await Client.PostAsync(Endpoint, null);

        // Assert
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<CommissionCalculationResponse>>();
        Assert.NotNull(body?.Data);

        // FakeiSeriesAdapter returns CommissionableGrossProfit=5000, TotalCommission=0
        Assert.Equal(5000m, body.Data.CommissionableGrossProfit);
        Assert.Equal(0m, body.Data.TotalCommission);
    }

    [Fact]
    public async Task Should_SetCommissionableGrossProfit()
    {
        // Arrange
        await ArrangeSaleWithSalesTeamAsync();
        await Fixture.SeedFundingRequestCacheAsync(SaleId, PackageId);

        // Act
        var response = await Client.PostAsync(Endpoint, null);
        response.EnsureSuccessStatusCode();

        // Assert — CGP should be persisted on the package
        var package = await GetPackageAsync();
        Assert.Equal(5000m, package.CommissionableGrossProfit);
    }
}
