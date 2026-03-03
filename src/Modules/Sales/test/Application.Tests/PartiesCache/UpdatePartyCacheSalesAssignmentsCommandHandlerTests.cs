using Modules.Sales.Application.PartiesCache.UpdatePartyCacheSalesAssignments;
using Modules.Sales.Domain.PartiesCache;
using NSubstitute;
using Rtl.Core.Application.Caching;
using Rtl.Core.Domain;
using Xunit;

namespace Modules.Sales.Application.Tests.PartiesCache;

public sealed class UpdatePartyCacheSalesAssignmentsCommandHandlerTests
{
    private readonly ICacheWriteScope _cacheWriteScope = Substitute.For<ICacheWriteScope>();
    private readonly IPartyCacheWriter _partyCacheWriter = Substitute.For<IPartyCacheWriter>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly UpdatePartyCacheSalesAssignmentsCommandHandler _sut;

    public UpdatePartyCacheSalesAssignmentsCommandHandlerTests()
    {
        _cacheWriteScope.AllowWrites().Returns(Substitute.For<IDisposable>());
        _dateTimeProvider.UtcNow.Returns(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
        _sut = new UpdatePartyCacheSalesAssignmentsCommandHandler(_cacheWriteScope, _partyCacheWriter, _dateTimeProvider);
    }

    [Fact]
    public async Task Returns_success_and_updates_sales_assignments()
    {
        var refPublicId = Guid.NewGuid();
        var command = new UpdatePartyCacheSalesAssignmentsCommand(
            RefPublicId: refPublicId,
            PrimaryFederatedId: "fed-001",
            PrimaryFirstName: "Alice",
            PrimaryLastName: "Smith",
            SecondaryFederatedId: "fed-002",
            SecondaryFirstName: "Bob",
            SecondaryLastName: "Jones");

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _partyCacheWriter.Received(1).UpdateSalesAssignmentsAsync(
            refPublicId, "fed-001", "Alice", "Smith", "fed-002", "Bob", "Jones",
            _dateTimeProvider.UtcNow, Arg.Any<CancellationToken>());
    }
}
