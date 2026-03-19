using Modules.Sales.Application.CustomersCache.UpsertCustomerCache;
using Modules.Sales.Domain.CustomersCache;
using NSubstitute;
using Rtl.Core.Application.Caching;
using Rtl.Core.Domain;
using Xunit;

namespace Modules.Sales.Application.Tests.CustomersCache;

public sealed class UpsertCustomerCacheCommandHandlerTests
{
    private readonly ICacheWriteScope _cacheWriteScope = Substitute.For<ICacheWriteScope>();
    private readonly ICustomerCacheWriter _customerCacheWriter = Substitute.For<ICustomerCacheWriter>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly UpsertCustomerCacheCommandHandler _sut;

    public UpsertCustomerCacheCommandHandlerTests()
    {
        _cacheWriteScope.AllowWrites().Returns(Substitute.For<IDisposable>());
        _dateTimeProvider.UtcNow.Returns(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
        _sut = new UpsertCustomerCacheCommandHandler(_cacheWriteScope, _customerCacheWriter, _dateTimeProvider);
    }

    [Fact]
    public async Task Returns_success_and_upserts_customer_cache()
    {
        var customerCache = new CustomerCache
        {
            RefPublicId = Guid.NewGuid(),
            DisplayName = "John Doe",
            FirstName = "John",
            LastName = "Doe"
        };

        var result = await _sut.Handle(
            new UpsertCustomerCacheCommand(customerCache), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _customerCacheWriter.Received(1).UpsertAsync(
            customerCache, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Sets_LastSyncedAtUtc_from_date_time_provider()
    {
        var customerCache = new CustomerCache { RefPublicId = Guid.NewGuid(), DisplayName = "Test" };

        await _sut.Handle(
            new UpsertCustomerCacheCommand(customerCache), CancellationToken.None);

        Assert.Equal(_dateTimeProvider.UtcNow, customerCache.LastSyncedAtUtc);
    }
}
