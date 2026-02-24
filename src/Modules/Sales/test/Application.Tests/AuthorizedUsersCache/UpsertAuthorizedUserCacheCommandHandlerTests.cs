using Modules.Sales.Application.AuthorizedUsersCache.UpsertAuthorizedUserCache;
using Modules.Sales.Domain.AuthorizedUsersCache;
using NSubstitute;
using Rtl.Core.Application.Caching;
using Rtl.Core.Domain;
using Xunit;

namespace Modules.Sales.Application.Tests.AuthorizedUsersCache;

public sealed class UpsertAuthorizedUserCacheCommandHandlerTests
{
    private readonly ICacheWriteScope _cacheWriteScope = Substitute.For<ICacheWriteScope>();
    private readonly IAuthorizedUserCacheWriter _cacheWriter = Substitute.For<IAuthorizedUserCacheWriter>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly UpsertAuthorizedUserCacheCommandHandler _sut;

    public UpsertAuthorizedUserCacheCommandHandlerTests()
    {
        _cacheWriteScope.AllowWrites().Returns(Substitute.For<IDisposable>());
        _dateTimeProvider.UtcNow.Returns(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
        _sut = new UpsertAuthorizedUserCacheCommandHandler(_cacheWriteScope, _cacheWriter, _dateTimeProvider);
    }

    [Fact]
    public async Task Returns_success_and_upserts_cache()
    {
        var cache = new AuthorizedUserCache
        {
            RefUserId = 1,
            FederatedId = "fed-001",
            EmployeeNumber = 12345,
            FirstName = "Alice",
            LastName = "Smith",
            DisplayName = "Alice Smith",
            IsActive = true,
            AuthorizedHomeCenters = [100, 200]
        };

        var result = await _sut.Handle(
            new UpsertAuthorizedUserCacheCommand(cache), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _cacheWriter.Received(1).UpsertAsync(cache, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Sets_LastSyncedAtUtc_from_date_time_provider()
    {
        var cache = new AuthorizedUserCache
        {
            RefUserId = 1,
            FederatedId = "fed-001",
            DisplayName = "Test"
        };

        await _sut.Handle(
            new UpsertAuthorizedUserCacheCommand(cache), CancellationToken.None);

        Assert.Equal(_dateTimeProvider.UtcNow, cache.LastSyncedAtUtc);
    }
}
