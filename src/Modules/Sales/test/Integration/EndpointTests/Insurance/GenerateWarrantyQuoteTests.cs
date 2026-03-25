using Modules.Sales.EndpointTests.Abstractions;
using Modules.Sales.Presentation.Endpoints.V1.Insurance;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.EndpointTests.Insurance;

public class GenerateWarrantyQuoteTests(SalesEndpointTestFixture fixture) : SalesEndpointTestBase(fixture)
{
    [Fact]
    public async Task Should_ReturnWarrantyQuote_WhenSaleHasRequiredData()
    {
        // Arrange — sale + delivery + home
        await ArrangeSaleWithHomeAsync();

        // Act
        var body = await Client.PostAsync<ApiEnvelope<WarrantyQuoteResponse>>(
            $"/api/v1/sales/{SaleId}/insurance/quote/warranty",
            new { });

        // Assert — FakeiSeriesAdapter returns Premium=0, SalesTaxPremium=0
        Assert.NotNull(body?.Data);
        Assert.Equal(0m, body.Data.Premium);
        Assert.Equal(0m, body.Data.SalesTaxPremium);
        Assert.True(body.Data.WarrantySelected);
    }
}
