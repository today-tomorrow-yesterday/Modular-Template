using Modules.Sales.Application.PartiesCache.UpdatePartyCacheCoBuyer;
using Modules.Sales.Domain.PartiesCache;
using NSubstitute;
using Rtl.Core.Application.Caching;
using Rtl.Core.Domain;
using Xunit;

namespace Modules.Sales.Application.Tests.PartiesCache;

public sealed class UpdatePartyCacheCoBuyerCommandHandlerTests
{
    private readonly ICacheWriteScope _cacheWriteScope = Substitute.For<ICacheWriteScope>();
    private readonly IPartyCacheWriter _partyCacheWriter = Substitute.For<IPartyCacheWriter>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly UpdatePartyCacheCoBuyerCommandHandler _sut;

    public UpdatePartyCacheCoBuyerCommandHandlerTests()
    {
        _cacheWriteScope.AllowWrites().Returns(Substitute.For<IDisposable>());
        _dateTimeProvider.UtcNow.Returns(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
        _sut = new UpdatePartyCacheCoBuyerCommandHandler(_cacheWriteScope, _partyCacheWriter, _dateTimeProvider);
    }

    [Fact]
    public async Task Returns_success_and_updates_co_buyer()
    {
        var command = new UpdatePartyCacheCoBuyerCommand(
            RefPartyId: 1, CoBuyerFirstName: "Jane", CoBuyerLastName: "Doe");

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _partyCacheWriter.Received(1).UpdateCoBuyerAsync(
            1, "Jane", "Doe", _dateTimeProvider.UtcNow, Arg.Any<CancellationToken>());
    }
}
