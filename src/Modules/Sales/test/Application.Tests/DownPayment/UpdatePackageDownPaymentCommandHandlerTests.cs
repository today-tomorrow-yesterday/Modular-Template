using Modules.Sales.Application.Packages.UpdatePackageDownPayment;
using Modules.Sales.Domain;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Credits;
using NSubstitute;
using Rtl.Core.Application.Persistence;
using Xunit;

namespace Modules.Sales.Application.Tests.DownPayment;

public sealed class UpdatePackageDownPaymentCommandHandlerTests
{
    private readonly IPackageRepository _packageRepository = Substitute.For<IPackageRepository>();
    private readonly IUnitOfWork<ISalesModule> _unitOfWork = Substitute.For<IUnitOfWork<ISalesModule>>();
    private readonly UpdatePackageDownPaymentCommandHandler _sut;

    public UpdatePackageDownPaymentCommandHandlerTests() => _sut = new UpdatePackageDownPaymentCommandHandler(_packageRepository, _unitOfWork);

    [Fact]
    public async Task Returns_failure_when_package_not_found()
    {
        // Arrange
        var publicId = Guid.NewGuid();
        _packageRepository.GetByPublicIdWithSaleContextAsync(publicId, Arg.Any<CancellationToken>())
            .Returns((Package?)null);

        // Act
        var result = await _sut.Handle(
            new UpdatePackageDownPaymentCommand(publicId, 500m), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Package.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Creates_down_payment_line_when_none_exists_and_amount_positive()
    {
        // Arrange
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        // Act
        var result = await _sut.Handle(
            new UpdatePackageDownPaymentCommand(package.PublicId, 5000m), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var credit = Assert.Single(package.Lines.OfType<CreditLine>());
        Assert.True(credit.IsDownPayment);
        Assert.Equal(5000m, credit.SalePrice);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Updates_existing_down_payment_when_amount_changes()
    {
        // Arrange
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        var existingLine = CreditLine.CreateDownPayment(package.Id, 3000m);
        package.AddLine(existingLine);

        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        // Act
        var result = await _sut.Handle(
            new UpdatePackageDownPaymentCommand(package.PublicId, 7500m), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var credit = Assert.Single(package.Lines.OfType<CreditLine>());
        Assert.Equal(7500m, credit.SalePrice);
        Assert.Equal(0m, credit.EstimatedCost);
        Assert.Equal(0m, credit.RetailSalePrice);
    }

    [Fact]
    public async Task Deletes_existing_down_payment_when_amount_is_zero()
    {
        // Arrange
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        var existingLine = CreditLine.CreateDownPayment(package.Id, 3000m);
        package.AddLine(existingLine);

        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        // Act
        var result = await _sut.Handle(
            new UpdatePackageDownPaymentCommand(package.PublicId, 0m), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(package.Lines.OfType<CreditLine>());
    }

    [Fact]
    public async Task No_op_when_no_existing_line_and_amount_is_zero()
    {
        // Arrange
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        // Act
        var result = await _sut.Handle(
            new UpdatePackageDownPaymentCommand(package.PublicId, 0m), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(package.Lines);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Result_contains_correct_gross_profit_values()
    {
        // Arrange
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        // Act
        var result = await _sut.Handle(
            new UpdatePackageDownPaymentCommand(package.PublicId, 1000m), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(package.GrossProfit, result.Value.GrossProfit);
        Assert.Equal(package.CommissionableGrossProfit, result.Value.CommissionableGrossProfit);
        Assert.Equal(package.MustRecalculateTaxes, result.Value.MustRecalculateTaxes);
    }

    [Fact]
    public async Task Down_payment_does_not_affect_gross_profit()
    {
        // Arrange
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);
        var gpBefore = package.GrossProfit;

        // Act
        var result = await _sut.Handle(
            new UpdatePackageDownPaymentCommand(package.PublicId, 10000m), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(gpBefore, result.Value.GrossProfit);
    }

    [Fact]
    public async Task Amount_is_rounded_to_two_decimal_places()
    {
        // Arrange
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        // Act
        await _sut.Handle(
            new UpdatePackageDownPaymentCommand(package.PublicId, 1234.5678m), CancellationToken.None);

        // Assert
        var credit = Assert.Single(package.Lines.OfType<CreditLine>());
        Assert.Equal(1234.57m, credit.SalePrice);
    }
}
