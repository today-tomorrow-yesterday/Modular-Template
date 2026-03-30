using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Modules.Sales.Application.InventoryCache.RemoveOnLotHomeCache;
using Modules.Sales.Domain.InventoryCache;
using Modules.Sales.Domain.Packages;
using NSubstitute;
using Rtl.Core.Application.Caching;
using Xunit;

namespace Modules.Sales.Application.Tests.InventoryCache;

public sealed class RemoveOnLotHomeCacheCommandHandlerTests
{
    private static readonly Guid TestPublicId = Guid.Parse("00000000-0000-0000-0000-000000000042");

    private readonly ICacheWriteScope _cacheWriteScope = Substitute.For<ICacheWriteScope>();
    private readonly IOnLotHomeCacheWriter _cacheWriter = Substitute.For<IOnLotHomeCacheWriter>();
    private readonly IInventoryCacheQueries _cacheQueries = Substitute.For<IInventoryCacheQueries>();
    private readonly IPackageRepository _packageRepository = Substitute.For<IPackageRepository>();
    private readonly ILogger<RemoveOnLotHomeCacheCommandHandler> _logger = NullLogger<RemoveOnLotHomeCacheCommandHandler>.Instance;
    private readonly RemoveOnLotHomeCacheCommandHandler _sut;

    public RemoveOnLotHomeCacheCommandHandlerTests()
    {
        _cacheWriteScope.AllowWrites().Returns(Substitute.For<IDisposable>());
        _sut = new RemoveOnLotHomeCacheCommandHandler(_cacheWriteScope, _cacheWriter, _cacheQueries, _packageRepository, _logger);
    }

    [Fact]
    public async Task Returns_success_and_marks_cache_removed_when_no_affected_lines()
    {
        _cacheQueries.GetPackageLinesForHomeByPublicIdAsync(TestPublicId, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<AffectedPackageLine>());

        var result = await _sut.Handle(
            new RemoveOnLotHomeCacheCommand(TestPublicId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _cacheWriter.Received(1).MarkAsRemovedByPublicIdAsync(TestPublicId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Marks_cache_removed_even_when_affected_lines_exist()
    {
        var affectedLines = new List<AffectedPackageLine>
        {
            new(PackageLineId: 1, PackageId: 10, SaleId: 100)
        };
        _cacheQueries.GetPackageLinesForHomeByPublicIdAsync(TestPublicId, Arg.Any<CancellationToken>())
            .Returns(affectedLines);

        // Package not found — should still mark cache as removed
        _packageRepository.GetByIdWithTrackingAsync(10, Arg.Any<CancellationToken>())
            .Returns((Package?)null);

        var result = await _sut.Handle(
            new RemoveOnLotHomeCacheCommand(TestPublicId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _cacheWriter.Received(1).MarkAsRemovedByPublicIdAsync(TestPublicId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Flags_affected_package_line_as_product_removed()
    {
        var package = Package.Create(saleId: 100, name: "Test Package", isPrimary: true);
        var homeLine = Domain.Packages.Home.HomeLine.Create(
            packageId: package.Id,
            salePrice: 80_000m,
            estimatedCost: 60_000m,
            retailSalePrice: 80_000m,
            responsibility: null,
            details: null);
        package.AddLine(homeLine);

        var affectedLines = new List<AffectedPackageLine>
        {
            new(PackageLineId: homeLine.Id, PackageId: package.Id, SaleId: 100)
        };
        _cacheQueries.GetPackageLinesForHomeByPublicIdAsync(TestPublicId, Arg.Any<CancellationToken>())
            .Returns(affectedLines);
        _packageRepository.GetByIdWithTrackingAsync(package.Id, Arg.Any<CancellationToken>())
            .Returns(package);

        var result = await _sut.Handle(
            new RemoveOnLotHomeCacheCommand(TestPublicId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(homeLine.IsProductRemovedFromInventory);
    }
}
