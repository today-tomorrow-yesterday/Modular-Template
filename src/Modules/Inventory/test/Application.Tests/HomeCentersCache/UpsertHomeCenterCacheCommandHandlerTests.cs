using Modules.Inventory.Application.HomeCentersCache.UpsertHomeCenterCache;
using Modules.Inventory.Domain.HomeCentersCache;
using NSubstitute;
using Rtl.Core.Application.Caching;
using Rtl.Core.Domain;
using Xunit;

namespace Modules.Inventory.Application.Tests.HomeCentersCache;

public sealed class UpsertHomeCenterCacheCommandHandlerTests
{
    private readonly ICacheWriteScope _cacheWriteScope = Substitute.For<ICacheWriteScope>();
    private readonly IHomeCenterCacheWriter _cacheWriter = Substitute.For<IHomeCenterCacheWriter>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly UpsertHomeCenterCacheCommandHandler _sut;

    public UpsertHomeCenterCacheCommandHandlerTests()
    {
        _cacheWriteScope.AllowWrites().Returns(Substitute.For<IDisposable>());
        _sut = new UpsertHomeCenterCacheCommandHandler(_cacheWriteScope, _cacheWriter, _dateTimeProvider);
    }

    [Fact]
    public async Task Returns_success_and_sets_LastSyncedAtUtc()
    {
        var now = new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        _dateTimeProvider.UtcNow.Returns(now);

        var cache = new HomeCenterCache { Id = 1, RefHomeCenterNumber = 100, LotName = "Test" };
        var command = new UpsertHomeCenterCacheCommand(cache);

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(now, cache.LastSyncedAtUtc);
        await _cacheWriter.Received(1).UpsertAsync(cache, Arg.Any<CancellationToken>());
    }
}
