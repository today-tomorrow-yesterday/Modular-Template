using Modules.Sales.Application.Packages.UpdatePackageProjectCosts;
using Modules.Sales.Domain;
using Modules.Sales.Domain.Cdc;
using Modules.Sales.Domain.Packages;
using NSubstitute;
using Rtl.Core.Application.Persistence;
using Xunit;

namespace Modules.Sales.Application.Tests.Packages;

public sealed class UpdatePackageProjectCostsCommandHandlerTests
{
    private readonly IPackageRepository _packageRepository = Substitute.For<IPackageRepository>();
    private readonly ICdcProjectCostQueries _cdcProjectCostQueries = Substitute.For<ICdcProjectCostQueries>();
    private readonly IUnitOfWork<ISalesModule> _unitOfWork = Substitute.For<IUnitOfWork<ISalesModule>>();
    private readonly UpdatePackageProjectCostsCommandHandler _sut;

    public UpdatePackageProjectCostsCommandHandlerTests()
    {
        _sut = new UpdatePackageProjectCostsCommandHandler(
            _packageRepository, _cdcProjectCostQueries, _unitOfWork);
    }

    [Fact]
    public async Task Returns_failure_when_package_not_found()
    {
        var publicId = Guid.NewGuid();
        _packageRepository.GetByPublicIdWithSaleContextAsync(publicId, Arg.Any<CancellationToken>())
            .Returns((Package?)null);

        var result = await _sut.Handle(
            new UpdatePackageProjectCostsCommand(publicId, []),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Package.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Returns_success_on_happy_path()
    {
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        // Set up CDC reference data with a matching category/item
        var category = new CdcProjectCostCategory
        {
            CategoryNumber = 5,
            Description = "Setup",
            Items = new List<CdcProjectCostItem>
            {
                new() { CategoryId = 5, ItemNumber = 1, Description = "Foundation Setup" }
            }
        };
        _cdcProjectCostQueries.GetCategoriesWithItemsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<CdcProjectCostCategory> { category });

        var items = new[]
        {
            new UpdateProjectCostItemRequest(
                CategoryId: 5,
                ItemId: 1,
                SalePrice: 2500m,
                EstimatedCost: 2000m,
                RetailSalePrice: 2500m,
                ShouldExcludeFromPricing: false)
        };

        var result = await _sut.Handle(
            new UpdatePackageProjectCostsCommand(package.PublicId, items),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
