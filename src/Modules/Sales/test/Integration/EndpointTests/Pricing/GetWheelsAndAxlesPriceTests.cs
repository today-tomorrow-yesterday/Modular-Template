using Modules.Sales.EndpointTests.Abstractions;
using Modules.Sales.Integration.Shared;
using Modules.Sales.Presentation.Endpoints.V1.Pricing;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.EndpointTests.Pricing;

public class GetWheelsAndAxlesPriceTests(SalesEndpointTestFixture fixture) : SalesEndpointTestBase(fixture)
{
    [Fact]
    public async Task Should_ReturnWheelsAndAxlesPrice_WhenSaleExists()
    {
        // Arrange
        await ArrangeSaleWithHomeAsync();

        var url = $"/api/v1/sales/{SaleId}/pricing/wheels-and-axles"
            + "?numberOfWheels=4"
            + "&numberOfAxles=2";

        // Act
        var body = await Client.GetAsync<ApiEnvelope<WheelsAndAxlesPriceResponse>>(url);

        // Assert — FakeiSeriesAdapter.CalculateWheelAndAxlePriceByCount returns SalePrice=500
        Assert.NotNull(body?.Data);
        Assert.Equal(FakeiSeriesAdapter.WaSalePrice, body.Data.WheelsAndAxlesPrice);
    }
}
