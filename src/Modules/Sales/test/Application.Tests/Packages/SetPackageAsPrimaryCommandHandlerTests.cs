using Modules.Sales.Application.Packages.SetPackageAsPrimary;
using Modules.Sales.Domain;
using Modules.Sales.Domain.Packages;
using NSubstitute;
using Rtl.Core.Application.Persistence;
using Xunit;

namespace Modules.Sales.Application.Tests.Packages;

public sealed class SetPackageAsPrimaryCommandHandlerTests
{
    private readonly IPackageRepository _packageRepository = Substitute.For<IPackageRepository>();
    private readonly IUnitOfWork<ISalesModule> _unitOfWork = Substitute.For<IUnitOfWork<ISalesModule>>();
    private readonly SetPackageAsPrimaryCommandHandler _sut;

    public SetPackageAsPrimaryCommandHandlerTests() =>
        _sut = new SetPackageAsPrimaryCommandHandler(_packageRepository, _unitOfWork);

    [Fact]
    public async Task Returns_failure_when_package_not_found()
    {
        var publicId = Guid.NewGuid();
        _packageRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>())
            .Returns((Package?)null);

        var result = await _sut.Handle(
            new SetPackageAsPrimaryCommand(publicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Package.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Returns_success_immediately_when_already_primary()
    {
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        _packageRepository.GetByPublicIdAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        var result = await _sut.Handle(
            new SetPackageAsPrimaryCommand(package.PublicId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        // Should not load siblings or save when already primary
        await _packageRepository.DidNotReceive().GetBySaleIdWithTrackingAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Returns_success_and_promotes_non_primary_package()
    {
        var primary = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        var alternate = Package.Create(saleId: 1, name: "Alternate", isPrimary: false);
        _packageRepository.GetByPublicIdAsync(alternate.PublicId, Arg.Any<CancellationToken>())
            .Returns(alternate);
        _packageRepository.GetBySaleIdWithTrackingAsync(alternate.SaleId, Arg.Any<CancellationToken>())
            .Returns(new List<Package> { primary, alternate });

        var result = await _sut.Handle(
            new SetPackageAsPrimaryCommand(alternate.PublicId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(alternate.IsPrimaryPackage);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
