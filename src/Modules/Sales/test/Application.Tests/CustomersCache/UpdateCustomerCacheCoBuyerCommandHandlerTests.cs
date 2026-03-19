using Modules.Sales.Application.CustomersCache.UpdateCustomerCacheCoBuyer;
using Modules.Sales.Domain.CustomersCache;
using NSubstitute;
using Rtl.Core.Application.Caching;
using Rtl.Core.Domain;
using Xunit;

namespace Modules.Sales.Application.Tests.CustomersCache;

public sealed class UpdateCustomerCacheCoBuyerCommandHandlerTests
{
    private readonly ICacheWriteScope _cacheWriteScope = Substitute.For<ICacheWriteScope>();
    private readonly ICustomerCacheWriter _customerCacheWriter = Substitute.For<ICustomerCacheWriter>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly UpdateCustomerCacheCoBuyerCommandHandler _sut;

    public UpdateCustomerCacheCoBuyerCommandHandlerTests()
    {
        _cacheWriteScope.AllowWrites().Returns(Substitute.For<IDisposable>());
        _dateTimeProvider.UtcNow.Returns(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
        _sut = new UpdateCustomerCacheCoBuyerCommandHandler(_cacheWriteScope, _customerCacheWriter, _dateTimeProvider);
    }

    [Fact]
    public async Task Returns_success_and_updates_co_buyer()
    {
        var refPublicId = Guid.NewGuid();
        var command = new UpdateCustomerCacheCoBuyerCommand(
            RefPublicId: refPublicId, CoBuyerFirstName: "Jane", CoBuyerLastName: "Doe");

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _customerCacheWriter.Received(1).UpdateCoBuyerAsync(
            refPublicId, "Jane", "Doe", _dateTimeProvider.UtcNow, Arg.Any<CancellationToken>());
    }
}
