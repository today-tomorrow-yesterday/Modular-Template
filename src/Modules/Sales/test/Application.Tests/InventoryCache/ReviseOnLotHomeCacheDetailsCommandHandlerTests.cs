using Modules.Sales.Application.InventoryCache.ReviseOnLotHomeCacheDetails;
using Modules.Sales.Domain.InventoryCache;
using NSubstitute;
using Rtl.Core.Application.Caching;
using Rtl.Core.Domain;
using Xunit;

namespace Modules.Sales.Application.Tests.InventoryCache;

public sealed class ReviseOnLotHomeCacheDetailsCommandHandlerTests
{
    private readonly ICacheWriteScope _cacheWriteScope = Substitute.For<ICacheWriteScope>();
    private readonly IOnLotHomeCacheWriter _cacheWriter = Substitute.For<IOnLotHomeCacheWriter>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly ReviseOnLotHomeCacheDetailsCommandHandler _sut;

    public ReviseOnLotHomeCacheDetailsCommandHandlerTests()
    {
        _cacheWriteScope.AllowWrites().Returns(Substitute.For<IDisposable>());
        _dateTimeProvider.UtcNow.Returns(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
        _sut = new ReviseOnLotHomeCacheDetailsCommandHandler(_cacheWriteScope, _cacheWriter, _dateTimeProvider);
    }

    [Fact]
    public async Task Returns_success_and_upserts_cache()
    {
        var cache = new OnLotHomeCache { RefOnLotHomeId = 1, Model = "Patriot" };

        var result = await _sut.Handle(
            new ReviseOnLotHomeCacheDetailsCommand(cache), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _cacheWriter.Received(1).UpsertAsync(cache, Arg.Any<CancellationToken>());
        Assert.Equal(_dateTimeProvider.UtcNow, cache.LastSyncedAtUtc);
    }
}
