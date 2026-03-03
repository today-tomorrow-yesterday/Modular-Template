using Modules.Sales.Application.PartiesCache.UpdatePartyCacheName;
using Modules.Sales.Domain.PartiesCache;
using NSubstitute;
using Rtl.Core.Application.Caching;
using Rtl.Core.Domain;
using Xunit;

namespace Modules.Sales.Application.Tests.PartiesCache;

public sealed class UpdatePartyCacheNameCommandHandlerTests
{
    private readonly ICacheWriteScope _cacheWriteScope = Substitute.For<ICacheWriteScope>();
    private readonly IPartyCacheWriter _partyCacheWriter = Substitute.For<IPartyCacheWriter>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly UpdatePartyCacheNameCommandHandler _sut;

    public UpdatePartyCacheNameCommandHandlerTests()
    {
        _cacheWriteScope.AllowWrites().Returns(Substitute.For<IDisposable>());
        _dateTimeProvider.UtcNow.Returns(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
        _sut = new UpdatePartyCacheNameCommandHandler(_cacheWriteScope, _partyCacheWriter, _dateTimeProvider);
    }

    [Fact]
    public async Task Returns_success_and_updates_name()
    {
        var refPublicId = Guid.NewGuid();
        var command = new UpdatePartyCacheNameCommand(
            RefPublicId: refPublicId,
            PartyType: PartyType.Person,
            DisplayName: "John Doe",
            FirstName: "John",
            MiddleName: null,
            LastName: "Doe",
            OrganizationName: null);

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _partyCacheWriter.Received(1).UpdateNameAsync(
            refPublicId, PartyType.Person, "John Doe", "John", null, "Doe", null,
            _dateTimeProvider.UtcNow, Arg.Any<CancellationToken>());
    }
}
