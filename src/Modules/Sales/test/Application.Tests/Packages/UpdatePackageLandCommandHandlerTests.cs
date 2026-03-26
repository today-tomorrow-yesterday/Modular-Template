using Modules.Sales.Application.Packages.UpdatePackageLand;
using Modules.Sales.Domain;
using Modules.Sales.Domain.InventoryCache;
using Modules.Sales.Domain.Packages;
using RetailLocationCacheEntity = Modules.Sales.Domain.RetailLocationCache.RetailLocationCache;
using Modules.Sales.Domain.Sales;
using NSubstitute;
using Rtl.Core.Application.Persistence;
using System.Reflection;
using Xunit;

namespace Modules.Sales.Application.Tests.Packages;

public sealed class UpdatePackageLandCommandHandlerTests
{
    private readonly IPackageRepository _packageRepository = Substitute.For<IPackageRepository>();
    private readonly IInventoryCacheQueries _inventoryCacheQueries = Substitute.For<IInventoryCacheQueries>();
    private readonly IUnitOfWork<ISalesModule> _unitOfWork = Substitute.For<IUnitOfWork<ISalesModule>>();
    private readonly UpdatePackageLandCommandHandler _sut;

    public UpdatePackageLandCommandHandlerTests() =>
        _sut = new UpdatePackageLandCommandHandler(_packageRepository, _inventoryCacheQueries, _unitOfWork);

    [Fact]
    public async Task Returns_failure_when_package_not_found()
    {
        var publicId = Guid.NewGuid();
        _packageRepository.GetByPublicIdWithSaleContextAsync(publicId, Arg.Any<CancellationToken>())
            .Returns((Package?)null);

        var result = await _sut.Handle(CreateCommand(publicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Package.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Returns_success_on_happy_path()
    {
        var package = CreatePackageWithSaleContext();
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        var result = await _sut.Handle(CreateCommand(package.PublicId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // --- Helpers ---

    private static UpdatePackageLandCommand CreateCommand(Guid packagePublicId) =>
        new(
            PackagePublicId: packagePublicId,
            SalePrice: 50000m,
            EstimatedCost: 45000m,
            RetailSalePrice: 55000m,
            LandPurchaseType: "CustomerWantsToPurchaseLand",
            TypeOfLandWanted: "LandPurchase",
            CustomerLandType: null,
            LandInclusion: null,
            LandStockNumber: null,
            LandSalesPrice: null,
            LandCost: null,
            PropertyOwner: null,
            FinancedBy: null,
            EstimatedValue: null,
            SizeInAcres: null,
            PayoffAmountFinancing: null,
            LandEquity: null,
            OriginalPurchaseDate: null,
            OriginalPurchasePrice: null,
            Realtor: null,
            PurchasePrice: 50000m,
            PropertyOwnerPhoneNumber: null,
            PropertyLotRent: null,
            CommunityNumber: null,
            CommunityName: null,
            CommunityManagerName: null,
            CommunityManagerPhoneNumber: null,
            CommunityManagerEmail: null,
            CommunityMonthlyCost: null);

    private static Package CreatePackageWithSaleContext()
    {
        var sale = Sale.Create(customerId: 1, retailLocationId: 1, saleType: SaleType.B2C);
        sale.ClearDomainEvents();

        var retailLocation = RetailLocationCacheEntity.CreateHomeCenter(
            homeCenterNumber: 42, name: "Test HC", stateCode: "OH", zip: "43004", isActive: true);
        SetProperty(sale, nameof(Sale.RetailLocation), retailLocation);

        var package = Package.Create(saleId: sale.Id, name: "Primary", isPrimary: true);
        package.ClearDomainEvents();
        SetProperty(package, nameof(Package.Sale), sale);

        return package;
    }

    private static void SetProperty(object obj, string propertyName, object? value)
    {
        var backingField = obj.GetType().GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
        if (backingField is not null)
        {
            backingField.SetValue(obj, value);
        }
        else
        {
            var prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)!;
            prop.SetValue(obj, value);
        }
    }
}
