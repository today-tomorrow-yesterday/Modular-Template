using Modules.Sales.EndpointTests.Abstractions;
using Modules.Sales.Presentation.Endpoints.V1.Tax;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.EndpointTests.Tax;

// This endpoint reads from CDC tax_exemption table.
// Without CDC seed data, it returns an empty list.
public class GetTaxExemptionsTests(SalesEndpointTestFixture fixture) : SalesEndpointTestBase(fixture)
{
    [Fact]
    public async Task Should_ReturnEmptyList_WhenNoCdcDataSeeded()
    {
        // Arrange — no CDC seed data; endpoint should still succeed

        // Act
        var body = await Client.GetAsync<ApiEnvelope<List<TaxExemptionResponse>>>(
            "/api/v1/sales/tax/exemptions");

        // Assert — returns empty list since no CDC data is seeded
        // NOTE: When CDC seed data is added, update this test to verify actual exemptions.
        Assert.NotNull(body?.Data);
        Assert.Empty(body.Data);
    }
}
