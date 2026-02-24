using Modules.Sales.Application.Packages.DeletePackage;
using Modules.Sales.Domain;
using Modules.Sales.Domain.Packages;
using NSubstitute;
using Rtl.Core.Application.Persistence;
using Xunit;

namespace Modules.Sales.Application.Tests.Packages;

public sealed class DeletePackageCommandHandlerTests
{
    private readonly IPackageRepository _packageRepository = Substitute.For<IPackageRepository>();
    private readonly IUnitOfWork<ISalesModule> _unitOfWork = Substitute.For<IUnitOfWork<ISalesModule>>();
    private readonly DeletePackageCommandHandler _sut;

    public DeletePackageCommandHandlerTests() =>
        _sut = new DeletePackageCommandHandler(_packageRepository, _unitOfWork);

    [Fact]
    public async Task Returns_failure_when_package_not_found()
    {
        var publicId = Guid.NewGuid();
        _packageRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>())
            .Returns((Package?)null);

        var result = await _sut.Handle(
            new DeletePackageCommand(publicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Package.NotFound", result.Error.Code);
    }

    // Note: PackageStatus.Draft guard (OnlyDraftCanBeDeleted) is unreachable with the
    // current enum — Draft is the only value. Test should be added when more statuses exist.

    [Fact]
    public async Task Returns_failure_when_sole_remaining_package()
    {
        var package = Package.Create(saleId: 1, name: "Alternate", isPrimary: false);
        _packageRepository.GetByPublicIdAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);
        _packageRepository.GetBySaleIdAsync(package.SaleId, Arg.Any<CancellationToken>())
            .Returns(new List<Package> { package });

        var result = await _sut.Handle(
            new DeletePackageCommand(package.PublicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Package.CannotDeleteLastPackage", result.Error.Code);
    }

    [Fact]
    public async Task Returns_failure_when_deleting_primary_with_siblings()
    {
        var primary = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        var alternate = Package.Create(saleId: 1, name: "Alternate", isPrimary: false);
        _packageRepository.GetByPublicIdAsync(primary.PublicId, Arg.Any<CancellationToken>())
            .Returns(primary);
        _packageRepository.GetBySaleIdAsync(primary.SaleId, Arg.Any<CancellationToken>())
            .Returns(new List<Package> { primary, alternate });

        var result = await _sut.Handle(
            new DeletePackageCommand(primary.PublicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Package.CannotDeletePrimary", result.Error.Code);
    }

    [Fact]
    public async Task Returns_success_when_deleting_non_primary_with_siblings()
    {
        var primary = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        var alternate = Package.Create(saleId: 1, name: "Alternate", isPrimary: false);
        _packageRepository.GetByPublicIdAsync(alternate.PublicId, Arg.Any<CancellationToken>())
            .Returns(alternate);
        _packageRepository.GetBySaleIdAsync(alternate.SaleId, Arg.Any<CancellationToken>())
            .Returns(new List<Package> { primary, alternate });

        var result = await _sut.Handle(
            new DeletePackageCommand(alternate.PublicId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _packageRepository.Received(1).Remove(alternate);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
