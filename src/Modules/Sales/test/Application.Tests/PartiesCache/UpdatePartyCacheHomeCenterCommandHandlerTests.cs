using Modules.Sales.Application.PartiesCache.UpdatePartyCacheHomeCenter;
using Modules.Sales.Domain.PartiesCache;
using NSubstitute;
using Rtl.Core.Application.Caching;
using Rtl.Core.Domain;
using Xunit;

namespace Modules.Sales.Application.Tests.PartiesCache;

public sealed class UpdatePartyCacheHomeCenterCommandHandlerTests
{
    private readonly ICacheWriteScope _cacheWriteScope = Substitute.For<ICacheWriteScope>();
    private readonly IPartyCacheWriter _partyCacheWriter = Substitute.For<IPartyCacheWriter>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly UpdatePartyCacheHomeCenterCommandHandler _sut;

    public UpdatePartyCacheHomeCenterCommandHandlerTests()
    {
        _cacheWriteScope.AllowWrites().Returns(Substitute.For<IDisposable>());
        _dateTimeProvider.UtcNow.Returns(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
        _sut = new UpdatePartyCacheHomeCenterCommandHandler(_cacheWriteScope, _partyCacheWriter, _dateTimeProvider);
    }

    [Fact]
    public async Task Returns_success_and_updates_home_center_number()
    {
        var command = new UpdatePartyCacheHomeCenterCommand(RefPartyId: 1, NewHomeCenterNumber: 200);

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _partyCacheWriter.Received(1).UpdateHomeCenterNumberAsync(
            1, 200, _dateTimeProvider.UtcNow, Arg.Any<CancellationToken>());
    }
}
