using Modules.Sales.Application.CustomersCache.UpdateCustomerCacheName;
using Modules.Sales.Domain.CustomersCache;
using NSubstitute;
using Rtl.Core.Application.Caching;
using Rtl.Core.Domain;
using Xunit;

namespace Modules.Sales.Application.Tests.CustomersCache;

public sealed class UpdateCustomerCacheNameCommandHandlerTests
{
    private readonly ICacheWriteScope _cacheWriteScope = Substitute.For<ICacheWriteScope>();
    private readonly ICustomerCacheWriter _customerCacheWriter = Substitute.For<ICustomerCacheWriter>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly UpdateCustomerCacheNameCommandHandler _sut;

    public UpdateCustomerCacheNameCommandHandlerTests()
    {
        _cacheWriteScope.AllowWrites().Returns(Substitute.For<IDisposable>());
        _dateTimeProvider.UtcNow.Returns(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
        _sut = new UpdateCustomerCacheNameCommandHandler(_cacheWriteScope, _customerCacheWriter, _dateTimeProvider);
    }

    [Fact]
    public async Task Returns_success_and_updates_name()
    {
        var refPublicId = Guid.NewGuid();
        var command = new UpdateCustomerCacheNameCommand(
            RefPublicId: refPublicId,
            DisplayName: "John Doe",
            FirstName: "John",
            MiddleName: null,
            LastName: "Doe");

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _customerCacheWriter.Received(1).UpdateNameAsync(
            refPublicId, "John Doe", "John", null, "Doe",
            _dateTimeProvider.UtcNow, Arg.Any<CancellationToken>());
    }
}
