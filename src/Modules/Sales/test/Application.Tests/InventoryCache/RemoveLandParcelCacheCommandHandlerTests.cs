using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Modules.Sales.Application.InventoryCache.RemoveLandParcelCache;
using Modules.Sales.Domain.InventoryCache;
using NSubstitute;
using Rtl.Core.Application.Caching;
using Xunit;

namespace Modules.Sales.Application.Tests.InventoryCache;

public sealed class RemoveLandParcelCacheCommandHandlerTests
{
    private readonly ICacheWriteScope _cacheWriteScope = Substitute.For<ICacheWriteScope>();
    private readonly ILandParcelCacheWriter _cacheWriter = Substitute.For<ILandParcelCacheWriter>();
    private readonly IInventoryCacheQueries _cacheQueries = Substitute.For<IInventoryCacheQueries>();
    private readonly ILogger<RemoveLandParcelCacheCommandHandler> _logger = NullLogger<RemoveLandParcelCacheCommandHandler>.Instance;
    private readonly RemoveLandParcelCacheCommandHandler _sut;

    public RemoveLandParcelCacheCommandHandlerTests()
    {
        _cacheWriteScope.AllowWrites().Returns(Substitute.For<IDisposable>());
        _sut = new RemoveLandParcelCacheCommandHandler(_cacheWriteScope, _cacheWriter, _cacheQueries, _logger);
    }

    [Fact]
    public async Task Returns_success_and_removes_cache_when_no_affected_lines()
    {
        _cacheQueries.GetPackageLinesForLandByRefIdAsync(42, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<AffectedPackageLine>());

        var result = await _sut.Handle(
            new RemoveLandParcelCacheCommand(42, 100, "LP-100"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _cacheWriter.Received(1).RemoveByRefIdAsync(42, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Removes_cache_even_when_affected_lines_exist()
    {
        var affectedLines = new List<AffectedPackageLine>
        {
            new(PackageLineId: 1, PackageId: 10, SaleId: 100)
        };
        _cacheQueries.GetPackageLinesForLandByRefIdAsync(42, Arg.Any<CancellationToken>())
            .Returns(affectedLines);

        var result = await _sut.Handle(
            new RemoveLandParcelCacheCommand(42, 100, "LP-100"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _cacheWriter.Received(1).RemoveByRefIdAsync(42, Arg.Any<CancellationToken>());
    }
}
