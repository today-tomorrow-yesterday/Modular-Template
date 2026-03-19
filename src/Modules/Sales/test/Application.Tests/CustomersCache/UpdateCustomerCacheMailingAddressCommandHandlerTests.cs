using Modules.Sales.Application.CustomersCache.UpdateCustomerCacheMailingAddress;
using Modules.Sales.Domain.CustomersCache;
using NSubstitute;
using Rtl.Core.Application.Caching;
using Rtl.Core.Domain;
using Xunit;

namespace Modules.Sales.Application.Tests.CustomersCache;

public sealed class UpdateCustomerCacheMailingAddressCommandHandlerTests
{
    private readonly ICacheWriteScope _cacheWriteScope = Substitute.For<ICacheWriteScope>();
    private readonly ICustomerCacheWriter _customerCacheWriter = Substitute.For<ICustomerCacheWriter>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly UpdateCustomerCacheMailingAddressCommandHandler _sut;

    public UpdateCustomerCacheMailingAddressCommandHandlerTests()
    {
        _cacheWriteScope.AllowWrites().Returns(Substitute.For<IDisposable>());
        _dateTimeProvider.UtcNow.Returns(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
        _sut = new UpdateCustomerCacheMailingAddressCommandHandler(_cacheWriteScope, _customerCacheWriter, _dateTimeProvider);
    }

    [Fact]
    public async Task Returns_success_and_updates_mailing_address_timestamp()
    {
        var refPublicId = Guid.NewGuid();
        var command = new UpdateCustomerCacheMailingAddressCommand(RefPublicId: refPublicId);

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _customerCacheWriter.Received(1).UpdateMailingAddressAsync(
            refPublicId, _dateTimeProvider.UtcNow, Arg.Any<CancellationToken>());
    }
}
