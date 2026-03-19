using Modules.Sales.Application.Packages.GetPackagesBySale;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Sales;
using NSubstitute;
using Xunit;

namespace Modules.Sales.Application.Tests.Packages;

public sealed class GetPackagesBySaleQueryHandlerTests
{
    private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
    private readonly IPackageRepository _packageRepository = Substitute.For<IPackageRepository>();
    private readonly GetPackagesBySaleQueryHandler _sut;

    public GetPackagesBySaleQueryHandlerTests() =>
        _sut = new GetPackagesBySaleQueryHandler(_saleRepository, _packageRepository);

    [Fact]
    public async Task Returns_failure_when_sale_not_found()
    {
        var publicId = Guid.NewGuid();
        _saleRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);

        var result = await _sut.Handle(
            new GetPackagesBySaleQuery(publicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Sale.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Returns_success_with_packages_when_sale_found()
    {
        var sale = Sale.Create(customerId: 1, retailLocationId: 1, saleType: SaleType.B2C, saleNumber: 100);
        sale.ClearDomainEvents();

        _saleRepository.GetByPublicIdAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);

        var package = Package.Create(saleId: sale.Id, name: "Primary", isPrimary: true);
        _packageRepository.GetBySaleIdAsync(sale.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Package> { package });

        var result = await _sut.Handle(
            new GetPackagesBySaleQuery(sale.PublicId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
    }
}
