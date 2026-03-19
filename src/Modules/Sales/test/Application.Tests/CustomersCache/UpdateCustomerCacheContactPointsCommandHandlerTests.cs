using Modules.Sales.Application.CustomersCache.UpdateCustomerCacheContactPoints;
using Modules.Sales.Domain.CustomersCache;
using NSubstitute;
using Rtl.Core.Application.Caching;
using Rtl.Core.Domain;
using Xunit;

namespace Modules.Sales.Application.Tests.CustomersCache;

public sealed class UpdateCustomerCacheContactPointsCommandHandlerTests
{
    private readonly ICacheWriteScope _cacheWriteScope = Substitute.For<ICacheWriteScope>();
    private readonly ICustomerCacheWriter _customerCacheWriter = Substitute.For<ICustomerCacheWriter>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly UpdateCustomerCacheContactPointsCommandHandler _sut;

    public UpdateCustomerCacheContactPointsCommandHandlerTests()
    {
        _cacheWriteScope.AllowWrites().Returns(Substitute.For<IDisposable>());
        _dateTimeProvider.UtcNow.Returns(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
        _sut = new UpdateCustomerCacheContactPointsCommandHandler(_cacheWriteScope, _customerCacheWriter, _dateTimeProvider);
    }

    [Fact]
    public async Task Returns_success_and_updates_contact_points()
    {
        var refPublicId = Guid.NewGuid();
        var command = new UpdateCustomerCacheContactPointsCommand(
            RefPublicId: refPublicId, Email: "john@test.com", Phone: "5551234567");

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _customerCacheWriter.Received(1).UpdateContactPointsAsync(
            refPublicId, "john@test.com", "5551234567", _dateTimeProvider.UtcNow, Arg.Any<CancellationToken>());
    }
}
