using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Modules.Sales.Application.InventoryCache.RemoveLandParcelCache;
using Modules.Sales.Domain.InventoryCache;
using Modules.Sales.Domain.Packages;
using NSubstitute;
using Rtl.Core.Application.Caching;
using Xunit;

namespace Modules.Sales.Application.Tests.InventoryCache;

public sealed class RemoveLandParcelCacheCommandHandlerTests
{
    private readonly ICacheWriteScope _cacheWriteScope = Substitute.For<ICacheWriteScope>();
    private readonly ILandParcelCacheWriter _cacheWriter = Substitute.For<ILandParcelCacheWriter>();
    private readonly IInventoryCacheQueries _cacheQueries = Substitute.For<IInventoryCacheQueries>();
    private readonly IPackageRepository _packageRepository = Substitute.For<IPackageRepository>();
    private readonly ILogger<RemoveLandParcelCacheCommandHandler> _logger = NullLogger<RemoveLandParcelCacheCommandHandler>.Instance;
    private readonly RemoveLandParcelCacheCommandHandler _sut;

    public RemoveLandParcelCacheCommandHandlerTests()
    {
        _cacheWriteScope.AllowWrites().Returns(Substitute.For<IDisposable>());
        _sut = new RemoveLandParcelCacheCommandHandler(_cacheWriteScope, _cacheWriter, _cacheQueries, _packageRepository, _logger);
    }

    [Fact]
    public async Task Returns_success_and_marks_cache_removed_when_no_affected_lines()
    {
        _cacheQueries.GetPackageLinesForLandByRefIdAsync(42, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<AffectedPackageLine>());

        var result = await _sut.Handle(
            new RemoveLandParcelCacheCommand(42, 100, "LP-100"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _cacheWriter.Received(1).MarkAsRemovedByRefIdAsync(42, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Marks_cache_removed_even_when_affected_lines_exist()
    {
        var affectedLines = new List<AffectedPackageLine>
        {
            new(PackageLineId: 1, PackageId: 10, SaleId: 100)
        };
        _cacheQueries.GetPackageLinesForLandByRefIdAsync(42, Arg.Any<CancellationToken>())
            .Returns(affectedLines);

        // Package not found — should still mark cache as removed
        _packageRepository.GetByIdWithTrackingAsync(10, Arg.Any<CancellationToken>())
            .Returns((Package?)null);

        var result = await _sut.Handle(
            new RemoveLandParcelCacheCommand(42, 100, "LP-100"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _cacheWriter.Received(1).MarkAsRemovedByRefIdAsync(42, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Flags_affected_package_line_as_product_removed()
    {
        var package = Package.Create(saleId: 100, name: "Test Package", isPrimary: true);
        var landLine = Domain.Packages.Land.LandLine.Create(
            packageId: package.Id,
            salePrice: 50_000m,
            estimatedCost: 40_000m,
            retailSalePrice: 50_000m,
            responsibility: null,
            details: null);
        package.AddLine(landLine);

        var affectedLines = new List<AffectedPackageLine>
        {
            new(PackageLineId: landLine.Id, PackageId: package.Id, SaleId: 100)
        };
        _cacheQueries.GetPackageLinesForLandByRefIdAsync(42, Arg.Any<CancellationToken>())
            .Returns(affectedLines);
        _packageRepository.GetByIdWithTrackingAsync(package.Id, Arg.Any<CancellationToken>())
            .Returns(package);

        var result = await _sut.Handle(
            new RemoveLandParcelCacheCommand(42, 100, "LP-100"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(landLine.IsProductRemovedFromInventory);
    }
}
