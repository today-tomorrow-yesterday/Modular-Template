using Modules.Sales.EndpointTests.Abstractions;
using Modules.Sales.Presentation.Endpoints.V1.Pricing;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.EndpointTests.Pricing;

public class GetRetailPriceTests(SalesEndpointTestFixture fixture) : SalesEndpointTestBase(fixture)
{
    [Fact]
    public async Task Should_ReturnRetailPrice_WhenSaleExists()
    {
        // Arrange
        await ArrangeSaleWithHomeAsync();

        var url = $"/api/v1/sales/{SaleId}/pricing/retail-price"
            + "?serialNumber=TEST123AB"
            + "&invoiceTotal=60000"
            + "&numberOfAxles=2"
            + "&hbgOptionTotal=1000"
            + "&retailOptionTotal=1500"
            + "&modelNumber=TestModel"
            + "&baseCost=48000"
            + "&effectiveDate=2026-01-01";

        // Act
        var body = await Client.GetAsync<ApiEnvelope<RetailPriceResponse>>(url);

        // Assert — FakeiSeriesAdapter.CalculateRetailPrice returns 0m
        Assert.NotNull(body?.Data);
        Assert.Equal(0m, body.Data.RetailPrice);
    }
}
