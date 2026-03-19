using Modules.Sales.Application.Packages.CreatePackage;
using Modules.Sales.Domain;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Sales;
using NSubstitute;
using Rtl.Core.Application.Persistence;
using Xunit;

namespace Modules.Sales.Application.Tests.Packages;

public sealed class CreatePackageCommandHandlerTests
{
    private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
    private readonly IPackageRepository _packageRepository = Substitute.For<IPackageRepository>();
    private readonly IUnitOfWork<ISalesModule> _unitOfWork = Substitute.For<IUnitOfWork<ISalesModule>>();
    private readonly CreatePackageCommandHandler _sut;

    public CreatePackageCommandHandlerTests() =>
        _sut = new CreatePackageCommandHandler(_saleRepository, _packageRepository, _unitOfWork);

    [Fact]
    public async Task Returns_failure_when_sale_not_found()
    {
        var salePublicId = Guid.NewGuid();
        _saleRepository.GetByPublicIdAsync(salePublicId, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);

        var result = await _sut.Handle(
            new CreatePackageCommand(salePublicId, "Primary"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Sale.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Returns_failure_when_duplicate_name_exists()
    {
        var sale = Sale.Create(customerId: 1, retailLocationId: 1, saleType: SaleType.B2C, saleNumber: 12345);
        sale.ClearDomainEvents();
        _saleRepository.GetByPublicIdAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);

        var existing = Package.Create(saleId: sale.Id, name: "Primary", isPrimary: true);
        _packageRepository.GetBySaleIdAsync(sale.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Package> { existing });

        var result = await _sut.Handle(
            new CreatePackageCommand(sale.PublicId, "Primary"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Package.DuplicateName", result.Error.Code);
    }

    [Fact]
    public async Task Returns_success_on_happy_path()
    {
        var sale = Sale.Create(customerId: 1, retailLocationId: 1, saleType: SaleType.B2C, saleNumber: 12345);
        sale.ClearDomainEvents();
        _saleRepository.GetByPublicIdAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);
        _packageRepository.GetBySaleIdAsync(sale.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Package>());

        var result = await _sut.Handle(
            new CreatePackageCommand(sale.PublicId, "Primary"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _packageRepository.Received(1).Add(Arg.Any<Package>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
