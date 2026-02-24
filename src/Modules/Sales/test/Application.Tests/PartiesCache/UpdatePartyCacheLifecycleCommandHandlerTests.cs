using Modules.Sales.Application.PartiesCache.UpdatePartyCacheLifecycle;
using Modules.Sales.Domain.PartiesCache;
using NSubstitute;
using Rtl.Core.Application.Caching;
using Rtl.Core.Domain;
using Xunit;

namespace Modules.Sales.Application.Tests.PartiesCache;

public sealed class UpdatePartyCacheLifecycleCommandHandlerTests
{
    private readonly ICacheWriteScope _cacheWriteScope = Substitute.For<ICacheWriteScope>();
    private readonly IPartyCacheWriter _partyCacheWriter = Substitute.For<IPartyCacheWriter>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly UpdatePartyCacheLifecycleCommandHandler _sut;

    public UpdatePartyCacheLifecycleCommandHandlerTests()
    {
        _cacheWriteScope.AllowWrites().Returns(Substitute.For<IDisposable>());
        _dateTimeProvider.UtcNow.Returns(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
        _sut = new UpdatePartyCacheLifecycleCommandHandler(_cacheWriteScope, _partyCacheWriter, _dateTimeProvider);
    }

    [Fact]
    public async Task Returns_success_and_updates_lifecycle_stage()
    {
        var command = new UpdatePartyCacheLifecycleCommand(
            RefPartyId: 1, NewLifecycleStage: LifecycleStage.Customer);

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _partyCacheWriter.Received(1).UpdateLifecycleStageAsync(
            1, LifecycleStage.Customer, _dateTimeProvider.UtcNow, Arg.Any<CancellationToken>());
    }
}
