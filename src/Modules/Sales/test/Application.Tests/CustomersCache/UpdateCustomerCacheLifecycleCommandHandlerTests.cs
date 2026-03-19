using Modules.Sales.Application.CustomersCache.UpdateCustomerCacheLifecycle;
using Modules.Sales.Domain.CustomersCache;
using NSubstitute;
using Rtl.Core.Application.Caching;
using Rtl.Core.Domain;
using Xunit;

namespace Modules.Sales.Application.Tests.CustomersCache;

public sealed class UpdateCustomerCacheLifecycleCommandHandlerTests
{
    private readonly ICacheWriteScope _cacheWriteScope = Substitute.For<ICacheWriteScope>();
    private readonly ICustomerCacheWriter _customerCacheWriter = Substitute.For<ICustomerCacheWriter>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly UpdateCustomerCacheLifecycleCommandHandler _sut;

    public UpdateCustomerCacheLifecycleCommandHandlerTests()
    {
        _cacheWriteScope.AllowWrites().Returns(Substitute.For<IDisposable>());
        _dateTimeProvider.UtcNow.Returns(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
        _sut = new UpdateCustomerCacheLifecycleCommandHandler(_cacheWriteScope, _customerCacheWriter, _dateTimeProvider);
    }

    [Fact]
    public async Task Returns_success_and_updates_lifecycle_stage()
    {
        var refPublicId = Guid.NewGuid();
        var command = new UpdateCustomerCacheLifecycleCommand(
            RefPublicId: refPublicId, NewLifecycleStage: LifecycleStage.Customer);

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _customerCacheWriter.Received(1).UpdateLifecycleStageAsync(
            refPublicId, LifecycleStage.Customer, _dateTimeProvider.UtcNow, Arg.Any<CancellationToken>());
    }
}
