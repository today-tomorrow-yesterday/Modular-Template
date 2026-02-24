using Modules.Sales.Application.InventoryCache.ReviseLandParcelCacheAppraisal;
using Modules.Sales.Domain.InventoryCache;
using NSubstitute;
using Rtl.Core.Application.Caching;
using Rtl.Core.Domain;
using Xunit;

namespace Modules.Sales.Application.Tests.InventoryCache;

public sealed class ReviseLandParcelCacheAppraisalCommandHandlerTests
{
    private readonly ICacheWriteScope _cacheWriteScope = Substitute.For<ICacheWriteScope>();
    private readonly ILandParcelCacheWriter _cacheWriter = Substitute.For<ILandParcelCacheWriter>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly ReviseLandParcelCacheAppraisalCommandHandler _sut;

    public ReviseLandParcelCacheAppraisalCommandHandlerTests()
    {
        _cacheWriteScope.AllowWrites().Returns(Substitute.For<IDisposable>());
        _dateTimeProvider.UtcNow.Returns(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
        _sut = new ReviseLandParcelCacheAppraisalCommandHandler(_cacheWriteScope, _cacheWriter, _dateTimeProvider);
    }

    [Fact]
    public async Task Returns_success_and_upserts_cache()
    {
        var cache = new LandParcelCache { RefLandParcelId = 1, Appraisal = 150_000m };

        var result = await _sut.Handle(
            new ReviseLandParcelCacheAppraisalCommand(cache), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _cacheWriter.Received(1).UpsertAsync(cache, Arg.Any<CancellationToken>());
        Assert.Equal(_dateTimeProvider.UtcNow, cache.LastSyncedAtUtc);
    }
}
