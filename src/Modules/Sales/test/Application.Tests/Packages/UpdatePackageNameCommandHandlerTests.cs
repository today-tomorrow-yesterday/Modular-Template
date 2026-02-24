using System.Reflection;
using Modules.Sales.Application.Packages.UpdatePackageName;
using Modules.Sales.Domain;
using Modules.Sales.Domain.Packages;
using NSubstitute;
using Rtl.Core.Application.Persistence;
using Xunit;

namespace Modules.Sales.Application.Tests.Packages;

public sealed class UpdatePackageNameCommandHandlerTests
{
    private readonly IPackageRepository _packageRepository = Substitute.For<IPackageRepository>();
    private readonly IUnitOfWork<ISalesModule> _unitOfWork = Substitute.For<IUnitOfWork<ISalesModule>>();
    private readonly UpdatePackageNameCommandHandler _sut;

    public UpdatePackageNameCommandHandlerTests() =>
        _sut = new UpdatePackageNameCommandHandler(_packageRepository, _unitOfWork);

    [Fact]
    public async Task Returns_failure_when_package_not_found()
    {
        var publicId = Guid.NewGuid();
        _packageRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>())
            .Returns((Package?)null);

        var result = await _sut.Handle(
            new UpdatePackageNameCommand(publicId, "New Name"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Package.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Returns_failure_when_duplicate_name_exists()
    {
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        SetId(package, 1);
        _packageRepository.GetByPublicIdAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        var sibling = Package.Create(saleId: 1, name: "Alternate", isPrimary: false);
        SetId(sibling, 2);
        _packageRepository.GetBySaleIdAsync(package.SaleId, Arg.Any<CancellationToken>())
            .Returns(new List<Package> { package, sibling });

        var result = await _sut.Handle(
            new UpdatePackageNameCommand(package.PublicId, "Alternate"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Package.DuplicateName", result.Error.Code);
    }

    private static void SetId(Package p, int id)
    {
        var type = typeof(Package);
        while (type is not null)
        {
            var field = type.GetField("<Id>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field is not null)
            {
                field.SetValue(p, id);
                return;
            }
            type = type.BaseType;
        }
    }

    [Fact]
    public async Task Returns_success_on_happy_path()
    {
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        _packageRepository.GetByPublicIdAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);
        _packageRepository.GetBySaleIdAsync(package.SaleId, Arg.Any<CancellationToken>())
            .Returns(new List<Package> { package });

        var result = await _sut.Handle(
            new UpdatePackageNameCommand(package.PublicId, "Renamed"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
