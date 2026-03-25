using Modules.Sales.EndpointTests.Abstractions;
using Modules.Sales.Presentation.Endpoints.V1.Pricing;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.EndpointTests.Pricing;

public class GetOptionTotalsTests(SalesEndpointTestFixture fixture) : SalesEndpointTestBase(fixture)
{
    [Fact]
    public async Task Should_ReturnOptionTotals_WhenSaleExists()
    {
        // Arrange
        await ArrangeSaleWithHomeAsync();

        var url = $"/api/v1/sales/{SaleId}/pricing/option-totals"
            + "?plantNumber=1"
            + "&quoteNumber=100"
            + "&orderNumber=200"
            + "&effectiveDate=2026-01-01";

        // Act
        var body = await Client.GetAsync<ApiEnvelope<OptionTotalsResponse>>(url);

        // Assert — FakeiSeriesAdapter.CalculateOptionTotals returns FactoryOptionTotal=0, RetailOptionTotal=0
        Assert.NotNull(body?.Data);
        Assert.Equal(0m, body.Data.HbgOptionTotal);
        Assert.Equal(0m, body.Data.RetailOptionTotal);
    }
}
