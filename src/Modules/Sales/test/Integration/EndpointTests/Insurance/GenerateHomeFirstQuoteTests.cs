using Modules.Sales.EndpointTests.Abstractions;
using Modules.Sales.Presentation.Endpoints.V1.Insurance;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.EndpointTests.Insurance;

public class GenerateHomeFirstQuoteTests(SalesEndpointTestFixture fixture) : SalesEndpointTestBase(fixture)
{
    [Fact]
    public async Task Should_ReturnHomeFirstQuote_WhenSaleHasRequiredData()
    {
        // Arrange — sale + delivery + home
        await ArrangeSaleWithHomeAsync();

        var request = new GenerateHomeFirstQuoteRequest(
            CoverageAmount: 300_000m,
            OccupancyType: 'P',
            IsHomeLocatedInPark: false,
            IsLandCustomerOwned: true,
            IsHomeOnPermanentFoundation: false,
            IsPremiumFinanced: true,
            CustomerBirthDate: new DateTime(1985, 6, 15),
            CoApplicantBirthDate: null,
            MailingAddress: "5000 Clayton Rd",
            MailingCity: "Maryville",
            MailingState: "TN",
            MailingZip: "37801");

        // Act
        var body = await Client.PostAsync<ApiEnvelope<HomeFirstQuoteResponse>>(
            $"/api/v1/sales/{SaleId}/insurance/quote/home-first",
            request);

        // Assert — FakeiSeriesAdapter returns InsuranceCompanyName="Test", TotalPremium=0, MaximumCoverage=0, TempLinkId=1
        Assert.NotNull(body?.Data);
        Assert.Equal("Test", body.Data.InsuranceCompanyName);
        Assert.Equal(1, body.Data.TempLinkId);
        Assert.Equal(0m, body.Data.Premium);
        Assert.Equal(0m, body.Data.MaxCoverage);
        Assert.True(body.Data.IsEligible);
        Assert.Null(body.Data.ErrorMessage);
    }
}
