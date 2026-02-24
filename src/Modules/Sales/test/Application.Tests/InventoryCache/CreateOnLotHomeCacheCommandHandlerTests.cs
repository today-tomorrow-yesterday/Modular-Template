using Modules.Sales.Application.InventoryCache.CreateOnLotHomeCache;
using Modules.Sales.Domain.InventoryCache;
using NSubstitute;
using Rtl.Core.Application.Caching;
using Rtl.Core.Domain;
using Xunit;

namespace Modules.Sales.Application.Tests.InventoryCache;

public sealed class CreateOnLotHomeCacheCommandHandlerTests
{
    private readonly ICacheWriteScope _cacheWriteScope = Substitute.For<ICacheWriteScope>();
    private readonly IOnLotHomeCacheWriter _cacheWriter = Substitute.For<IOnLotHomeCacheWriter>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly CreateOnLotHomeCacheCommandHandler _sut;

    public CreateOnLotHomeCacheCommandHandlerTests()
    {
        _cacheWriteScope.AllowWrites().Returns(Substitute.For<IDisposable>());
        _dateTimeProvider.UtcNow.Returns(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
        _sut = new CreateOnLotHomeCacheCommandHandler(_cacheWriteScope, _cacheWriter, _dateTimeProvider);
    }

    [Fact]
    public async Task Returns_success_and_upserts_cache()
    {
        var cache = new OnLotHomeCache { RefOnLotHomeId = 1, RefStockNumber = "OH-200" };

        var result = await _sut.Handle(
            new CreateOnLotHomeCacheCommand(cache), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _cacheWriter.Received(1).UpsertAsync(cache, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Sets_LastSyncedAtUtc_from_date_time_provider()
    {
        var cache = new OnLotHomeCache { RefOnLotHomeId = 1 };

        await _sut.Handle(new CreateOnLotHomeCacheCommand(cache), CancellationToken.None);

        Assert.Equal(_dateTimeProvider.UtcNow, cache.LastSyncedAtUtc);
    }
}
