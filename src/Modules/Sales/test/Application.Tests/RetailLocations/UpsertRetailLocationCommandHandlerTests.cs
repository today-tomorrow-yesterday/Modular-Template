using Modules.Sales.Application.RetailLocationCache.UpsertRetailLocationCache;
using Modules.Sales.Domain;
using Modules.Sales.Domain.RetailLocationCache;
using NSubstitute;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.Persistence;
using Xunit;
using RetailLocationCacheEntity = Modules.Sales.Domain.RetailLocationCache.RetailLocationCache;

namespace Modules.Sales.Application.Tests.RetailLocations;

public sealed class UpsertRetailLocationCommandHandlerTests
{
    private readonly IRetailLocationCacheRepository _retailLocationCacheRepository = Substitute.For<IRetailLocationCacheRepository>();
    private readonly ICacheWriteScope _cacheWriteScope = Substitute.For<ICacheWriteScope>();
    private readonly IUnitOfWork<ISalesModule> _unitOfWork = Substitute.For<IUnitOfWork<ISalesModule>>();
    private readonly UpsertRetailLocationCacheCommandHandler _sut;

    public UpsertRetailLocationCommandHandlerTests()
    {
        _cacheWriteScope.AllowWrites().Returns(Substitute.For<IDisposable>());
        _sut = new UpsertRetailLocationCacheCommandHandler(_retailLocationCacheRepository, _cacheWriteScope, _unitOfWork);
    }

    [Fact]
    public async Task Creates_new_retail_location_when_not_found()
    {
        _retailLocationCacheRepository.GetByHomeCenterNumberAsync(42, Arg.Any<CancellationToken>())
            .Returns((RetailLocationCacheEntity?)null);

        var result = await _sut.Handle(
            new UpsertRetailLocationCacheCommand(42, "Test HC", "OH", "43004", true),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        _retailLocationCacheRepository.Received(1).Add(Arg.Any<RetailLocationCacheEntity>());
    }

    [Fact]
    public async Task Updates_existing_retail_location_when_found()
    {
        var existing = RetailLocationCacheEntity.CreateHomeCenter(42, "Old Name", "OH", "43004", true);
        _retailLocationCacheRepository.GetByHomeCenterNumberAsync(42, Arg.Any<CancellationToken>())
            .Returns(existing);

        var result = await _sut.Handle(
            new UpsertRetailLocationCacheCommand(42, "New Name", "TX", "75001", false),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        _retailLocationCacheRepository.DidNotReceive().Add(Arg.Any<RetailLocationCacheEntity>());
        Assert.Equal("New Name", existing.Name);
    }
}
