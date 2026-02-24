using Modules.Sales.Application.PartiesCache.UpdatePartyCacheContactPoints;
using Modules.Sales.Domain.PartiesCache;
using NSubstitute;
using Rtl.Core.Application.Caching;
using Rtl.Core.Domain;
using Xunit;

namespace Modules.Sales.Application.Tests.PartiesCache;

public sealed class UpdatePartyCacheContactPointsCommandHandlerTests
{
    private readonly ICacheWriteScope _cacheWriteScope = Substitute.For<ICacheWriteScope>();
    private readonly IPartyCacheWriter _partyCacheWriter = Substitute.For<IPartyCacheWriter>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly UpdatePartyCacheContactPointsCommandHandler _sut;

    public UpdatePartyCacheContactPointsCommandHandlerTests()
    {
        _cacheWriteScope.AllowWrites().Returns(Substitute.For<IDisposable>());
        _dateTimeProvider.UtcNow.Returns(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
        _sut = new UpdatePartyCacheContactPointsCommandHandler(_cacheWriteScope, _partyCacheWriter, _dateTimeProvider);
    }

    [Fact]
    public async Task Returns_success_and_updates_contact_points()
    {
        var command = new UpdatePartyCacheContactPointsCommand(
            RefPartyId: 1, Email: "john@test.com", Phone: "5551234567");

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _partyCacheWriter.Received(1).UpdateContactPointsAsync(
            1, "john@test.com", "5551234567", _dateTimeProvider.UtcNow, Arg.Any<CancellationToken>());
    }
}
