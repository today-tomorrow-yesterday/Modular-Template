using Modules.Sales.Application.Packages.UpdatePackageInsurance;
using Modules.Sales.Domain;
using Modules.Sales.Domain.Packages;
using NSubstitute;
using Rtl.Core.Application.Persistence;
using Xunit;

namespace Modules.Sales.Application.Tests.Packages;

public sealed class UpdatePackageInsuranceCommandHandlerTests
{
    private readonly IPackageRepository _packageRepository = Substitute.For<IPackageRepository>();
    private readonly IUnitOfWork<ISalesModule> _unitOfWork = Substitute.For<IUnitOfWork<ISalesModule>>();
    private readonly UpdatePackageInsuranceCommandHandler _sut;

    public UpdatePackageInsuranceCommandHandlerTests() =>
        _sut = new UpdatePackageInsuranceCommandHandler(_packageRepository, _unitOfWork);

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
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        var result = await _sut.Handle(CreateCommand(package.PublicId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static UpdatePackageInsuranceCommand CreateCommand(Guid packagePublicId) =>
        new(
            PackagePublicId: packagePublicId,
            InsuranceType: "HomeFirst",
            CoverageAmount: 100000m,
            HasFoundationOrMasonry: false,
            InParkOrSubdivision: false,
            IsLandOwnedByCustomer: true,
            IsPremiumFinanced: true,
            QuoteId: "Q-001",
            CompanyName: "HomeFirst",
            MaxCoverage: 200000m,
            TotalPremium: 1200m);
}
