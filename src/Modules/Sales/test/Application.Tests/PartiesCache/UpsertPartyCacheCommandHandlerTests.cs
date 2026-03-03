using Modules.Sales.Application.PartiesCache.UpsertPartyCache;
using Modules.Sales.Domain.PartiesCache;
using NSubstitute;
using Rtl.Core.Application.Caching;
using Rtl.Core.Domain;
using Xunit;

namespace Modules.Sales.Application.Tests.PartiesCache;

public sealed class UpsertPartyCacheCommandHandlerTests
{
    private readonly ICacheWriteScope _cacheWriteScope = Substitute.For<ICacheWriteScope>();
    private readonly IPartyCacheWriter _partyCacheWriter = Substitute.For<IPartyCacheWriter>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly UpsertPartyCacheCommandHandler _sut;

    public UpsertPartyCacheCommandHandlerTests()
    {
        _cacheWriteScope.AllowWrites().Returns(Substitute.For<IDisposable>());
        _dateTimeProvider.UtcNow.Returns(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
        _sut = new UpsertPartyCacheCommandHandler(_cacheWriteScope, _partyCacheWriter, _dateTimeProvider);
    }

    [Fact]
    public async Task Returns_success_and_upserts_party_cache_with_person()
    {
        var partyCache = new PartyCache
        {
            RefPublicId = Guid.NewGuid(),
            PartyType = PartyType.Person,
            DisplayName = "John Doe"
        };
        var personCache = new PartyPersonCache { FirstName = "John", LastName = "Doe" };

        var result = await _sut.Handle(
            new UpsertPartyCacheCommand(partyCache, personCache, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _partyCacheWriter.Received(1).UpsertAsync(
            partyCache, personCache, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Sets_LastSyncedAtUtc_from_date_time_provider()
    {
        var partyCache = new PartyCache { RefPublicId = Guid.NewGuid(), DisplayName = "Test" };

        await _sut.Handle(
            new UpsertPartyCacheCommand(partyCache, null, null), CancellationToken.None);

        Assert.Equal(_dateTimeProvider.UtcNow, partyCache.LastSyncedAtUtc);
    }
}
