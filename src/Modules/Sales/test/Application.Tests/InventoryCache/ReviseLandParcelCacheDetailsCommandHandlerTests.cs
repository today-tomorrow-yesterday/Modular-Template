using Modules.Sales.Application.InventoryCache.ReviseLandParcelCacheDetails;
using Modules.Sales.Domain.InventoryCache;
using NSubstitute;
using Rtl.Core.Application.Caching;
using Rtl.Core.Domain;
using Xunit;

namespace Modules.Sales.Application.Tests.InventoryCache;

public sealed class ReviseLandParcelCacheDetailsCommandHandlerTests
{
    private readonly ICacheWriteScope _cacheWriteScope = Substitute.For<ICacheWriteScope>();
    private readonly ILandParcelCacheWriter _cacheWriter = Substitute.For<ILandParcelCacheWriter>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly ReviseLandParcelCacheDetailsCommandHandler _sut;

    public ReviseLandParcelCacheDetailsCommandHandlerTests()
    {
        _cacheWriteScope.AllowWrites().Returns(Substitute.For<IDisposable>());
        _dateTimeProvider.UtcNow.Returns(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
        _sut = new ReviseLandParcelCacheDetailsCommandHandler(_cacheWriteScope, _cacheWriter, _dateTimeProvider);
    }

    [Fact]
    public async Task Returns_success_and_upserts_cache()
    {
        var cache = new LandParcelCache { RefLandParcelId = 1, County = "Marion" };

        var result = await _sut.Handle(
            new ReviseLandParcelCacheDetailsCommand(cache), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _cacheWriter.Received(1).UpsertAsync(cache, Arg.Any<CancellationToken>());
        Assert.Equal(_dateTimeProvider.UtcNow, cache.LastSyncedAtUtc);
    }
}
