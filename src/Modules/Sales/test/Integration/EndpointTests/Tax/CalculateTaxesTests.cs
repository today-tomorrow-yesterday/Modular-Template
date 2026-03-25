using System.Net.Http.Json;
using Modules.Sales.EndpointTests.Abstractions;
using Modules.Sales.Presentation.Endpoints.V1.Tax;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.EndpointTests.Tax;

public class CalculateTaxesTests(SalesEndpointTestFixture fixture) : SalesEndpointTestBase(fixture)
{
    private string Endpoint => $"/api/v1/packages/{PackageId}/tax";

    [Fact]
    public async Task Should_CalculateTaxes_WhenPackageHasRequiredData()
    {
        // Arrange — sale + delivery + home + tax config + funding cache
        await ArrangeSaleWithTaxConfigAndFundingAsync();

        // Act
        var response = await Client.PostAsync(Endpoint, null);

        // Assert
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<TaxCalculationResponse>>();
        Assert.NotNull(body?.Data);

        // FakeiSeriesAdapter returns StateTax=1000, CityTax=200, CountyTax=150
        var taxItems = body.Data.TaxItems;
        Assert.NotEmpty(taxItems);

        var stateTax = Assert.Single(taxItems, t => t.Name == "State Tax");
        Assert.Equal(1000m, stateTax.CalculatedAmount);

        var cityTax = Assert.Single(taxItems, t => t.Name == "City Tax");
        Assert.Equal(200m, cityTax.CalculatedAmount);

        var countyTax = Assert.Single(taxItems, t => t.Name == "County Tax");
        Assert.Equal(150m, countyTax.CalculatedAmount);

        // Total tax sale price = 1000 + 200 + 150 = 1350
        Assert.Equal(1350m, body.Data.SalePrice);
    }

    [Fact]
    public async Task Should_ClearMustRecalculateTaxesFlag()
    {
        // Arrange
        await ArrangeSaleWithTaxConfigAndFundingAsync();

        // Verify the flag is true before calculation
        var packageBefore = await GetPackageAsync();
        Assert.True(packageBefore.MustRecalculateTaxes);

        // Act
        var response = await Client.PostAsync(Endpoint, null);
        response.EnsureSuccessStatusCode();

        // Assert — MustRecalculateTaxes should be false after successful calculation
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<TaxCalculationResponse>>();
        Assert.NotNull(body?.Data);
        Assert.False(body.Data.MustRecalculateTaxes);
    }
}
