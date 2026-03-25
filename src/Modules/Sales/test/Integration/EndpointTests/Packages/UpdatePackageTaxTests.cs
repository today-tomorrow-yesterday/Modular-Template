using System.Net;
using System.Net.Http.Json;
using Modules.Sales.EndpointTests.Abstractions;
using Modules.Sales.Presentation.Endpoints.V1.Tax;

namespace Modules.Sales.EndpointTests.Packages;

public class UpdatePackageTaxTests(SalesEndpointTestFixture fixture) : SalesEndpointTestBase(fixture)
{
    private string Endpoint => $"/api/v1/packages/{PackageId}/tax";

    [Fact]
    public async Task Tax_SetPreviouslyTitled()
    {
        // Arrange
        await ArrangeSaleWithHomeAsync();

        // Act
        var response = await Client.PutAsJsonAsync(Endpoint, new UpdatePackageTaxRequest(
            PreviouslyTitled: "Y",
            TaxExemptionId: null,
            QuestionAnswers: []));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);   // Should have returned 200 OK
        var updatedPackage = await GetPackageAsync();

        Assert.NotNull(updatedPackage.Tax);                     // Should create tax section
        Assert.Equal("Y", updatedPackage.Tax.PreviouslyTitled); // Should persist PreviouslyTitled as "Y"
        Assert.True(updatedPackage.MustRecalculateTaxes);       // Should flag taxes for recalculation
    }

    [Fact]
    public async Task Tax_ResaveSameConfig()
    {
        // Arrange
        await ArrangeSaleWithHomeAsync();

        var request = new UpdatePackageTaxRequest(
            PreviouslyTitled: "N",
            TaxExemptionId: null,
            QuestionAnswers: []);

        // Act — save same config twice
        var response1 = await Client.PutAsJsonAsync(Endpoint, request);
        var response2 = await Client.PutAsJsonAsync(Endpoint, request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);   // Should have returned 200 OK
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);   // Should have returned 200 OK
        var updatedPackage = await GetPackageAsync();

        Assert.True(updatedPackage.MustRecalculateTaxes);        // Should persist MustRecalculateTaxes flag
        Assert.Equal("N", updatedPackage.Tax!.PreviouslyTitled); // Should persist PreviouslyTitled as "N"
    }
}
