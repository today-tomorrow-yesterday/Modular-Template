using Modules.Sales.EndpointTests.Abstractions;
using Modules.Sales.Presentation.Endpoints.V1.ProjectCosts;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.EndpointTests.ProjectCosts;

// This endpoint reads from CDC project_cost_category/item/state_matrix tables.
// CDC data persists across test resets (not in the respawn-cleaned schemas).
public class GetProjectCostCategoriesTests(SalesEndpointTestFixture fixture) : SalesEndpointTestBase(fixture)
{
    [Fact]
    public async Task Should_ReturnCategories()
    {
        // Act
        var body = await Client.GetAsync<ApiEnvelope<ProjectCostCategoriesResponse>>(
            "/api/v1/sales/project-costs");

        // Assert — CDC data is available from migrations/seeding
        Assert.NotNull(body?.Data);
        Assert.NotNull(body.Data.Categories);
        Assert.NotNull(body.Data.StateMatrix);

        // Verify at least some categories are returned (CDC data persists)
        Assert.NotEmpty(body.Data.Categories);

        // Verify each category has expected structure
        foreach (var category in body.Data.Categories)
        {
            Assert.True(category.CategoryNumber > 0);
            Assert.NotNull(category.Description);
        }
    }
}
