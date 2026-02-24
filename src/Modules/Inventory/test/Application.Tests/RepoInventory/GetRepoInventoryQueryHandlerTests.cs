using Modules.Inventory.Application.RepoInventory.GetRepoInventory;
using Xunit;

namespace Modules.Inventory.Application.Tests.RepoInventory;

public sealed class GetRepoInventoryQueryHandlerTests
{
    private readonly GetRepoInventoryQueryHandler _sut = new();

    [Fact]
    public async Task Returns_success_with_empty_collection()
    {
        var query = new GetRepoInventoryQuery(35.0, -97.0, 200.0, 1);

        var result = await _sut.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }
}
