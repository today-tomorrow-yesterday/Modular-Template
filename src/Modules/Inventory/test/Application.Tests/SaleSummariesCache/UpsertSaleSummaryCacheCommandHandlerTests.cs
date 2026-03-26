using Modules.Inventory.Application.SaleSummariesCache.UpsertSaleSummaryCache;
using Modules.Inventory.Domain.SaleSummariesCache;
using NSubstitute;
using Rtl.Core.Application.Caching;
using Rtl.Core.Domain;
using Xunit;

namespace Modules.Inventory.Application.Tests.SaleSummariesCache;

public sealed class UpsertSaleSummaryCacheCommandHandlerTests
{
    private readonly ICacheWriteScope _cacheWriteScope = Substitute.For<ICacheWriteScope>();
    private readonly ISaleSummaryCacheWriter _cacheWriter = Substitute.For<ISaleSummaryCacheWriter>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly UpsertSaleSummaryCacheCommandHandler _sut;

    public UpsertSaleSummaryCacheCommandHandlerTests()
    {
        _cacheWriteScope.AllowWrites().Returns(Substitute.For<IDisposable>());
        _sut = new UpsertSaleSummaryCacheCommandHandler(_cacheWriteScope, _cacheWriter, _dateTimeProvider);
    }

    [Fact]
    public async Task Returns_success_and_sets_LastSyncedAtUtc()
    {
        var now = new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        _dateTimeProvider.UtcNow.Returns(now);

        var cache = new SaleSummaryCache { Id = 1, RefStockNumber = "STK-1", SalePublicId = Guid.NewGuid() };
        var command = new UpsertSaleSummaryCacheCommand(cache);

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(now, cache.LastSyncedAtUtc);
        await _cacheWriter.Received(1).UpsertAsync(cache, Arg.Any<CancellationToken>());
    }
}
