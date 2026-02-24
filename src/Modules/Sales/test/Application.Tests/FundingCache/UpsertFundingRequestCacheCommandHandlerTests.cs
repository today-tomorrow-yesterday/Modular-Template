using Modules.Sales.Application.FundingCache.UpsertFundingRequestCache;
using Modules.Sales.Domain.FundingCache;
using NSubstitute;
using Rtl.Core.Application.Caching;
using Rtl.Core.Domain;
using Xunit;

namespace Modules.Sales.Application.Tests.FundingCache;

public sealed class UpsertFundingRequestCacheCommandHandlerTests
{
    private readonly ICacheWriteScope _cacheWriteScope = Substitute.For<ICacheWriteScope>();
    private readonly IFundingRequestCacheWriter _cacheWriter = Substitute.For<IFundingRequestCacheWriter>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly UpsertFundingRequestCacheCommandHandler _sut;

    public UpsertFundingRequestCacheCommandHandlerTests()
    {
        _cacheWriteScope.AllowWrites().Returns(Substitute.For<IDisposable>());
        _dateTimeProvider.UtcNow.Returns(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
        _sut = new UpsertFundingRequestCacheCommandHandler(_cacheWriteScope, _cacheWriter, _dateTimeProvider);
    }

    [Fact]
    public async Task Returns_success_and_upserts_cache()
    {
        var cache = new FundingRequestCache
        {
            RefFundingRequestId = 1,
            SaleId = 10,
            PackageId = 20,
            Status = FundingRequestStatus.Pending,
            RequestAmount = 75_000m
        };

        var result = await _sut.Handle(
            new UpsertFundingRequestCacheCommand(cache), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _cacheWriter.Received(1).UpsertAsync(cache, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Sets_LastSyncedAtUtc_from_date_time_provider()
    {
        var cache = new FundingRequestCache
        {
            RefFundingRequestId = 1,
            SaleId = 10,
            PackageId = 20
        };

        await _sut.Handle(
            new UpsertFundingRequestCacheCommand(cache), CancellationToken.None);

        Assert.Equal(_dateTimeProvider.UtcNow, cache.LastSyncedAtUtc);
    }
}
