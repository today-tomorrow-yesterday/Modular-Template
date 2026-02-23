using Modules.Sales.Application.Packages.UpdatePackageConcessions;
using Modules.Sales.Domain;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Details;
using Modules.Sales.Domain.Packages.Lines;
using NSubstitute;
using Rtl.Core.Application.Persistence;
using Xunit;

namespace Modules.Sales.Application.Tests.Concessions;

public sealed class UpdatePackageConcessionsCommandHandlerTests
{
    private readonly IPackageRepository _packageRepository = Substitute.For<IPackageRepository>();
    private readonly IUnitOfWork<ISalesModule> _unitOfWork = Substitute.For<IUnitOfWork<ISalesModule>>();
    private readonly UpdatePackageConcessionsCommandHandler _sut;

    public UpdatePackageConcessionsCommandHandlerTests() => _sut = new UpdatePackageConcessionsCommandHandler(_packageRepository, _unitOfWork);

    [Fact]
    public async Task Returns_failure_when_package_not_found()
    {
        // Arrange
        var publicId = Guid.NewGuid();
        _packageRepository.GetByPublicIdWithSaleContextAsync(publicId, Arg.Any<CancellationToken>())
            .Returns((Package?)null);

        // Act
        var result = await _sut.Handle(
            new UpdatePackageConcessionsCommand(publicId, 500m), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Package.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Creates_concession_line_with_zero_estimated_cost()
    {
        // Arrange
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        // Act
        var result = await _sut.Handle(
            new UpdatePackageConcessionsCommand(package.PublicId, 5000m), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var credit = Assert.Single(package.Lines.OfType<CreditLine>());
        Assert.True(credit.IsConcession);
        Assert.Equal(5000m, credit.SalePrice);
        Assert.Equal(0m, credit.EstimatedCost);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Creates_seller_paid_closing_cost_project_cost_when_concession_added()
    {
        // Arrange
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        // Act
        await _sut.Handle(
            new UpdatePackageConcessionsCommand(package.PublicId, 3000m), CancellationToken.None);

        // Assert
        var pc = Assert.Single(package.Lines.OfType<ProjectCostLine>());
        Assert.Equal(0m, pc.SalePrice);
        Assert.Equal(3000m, pc.EstimatedCost);
        Assert.Equal(0m, pc.RetailSalePrice);
        Assert.False(pc.ShouldExcludeFromPricing);
        Assert.Equal(Responsibility.Seller, pc.Responsibility);
        Assert.Equal(14, pc.Details!.CategoryId);
        Assert.Equal(1, pc.Details.ItemId);
        Assert.Equal("Seller Paid Closing Cost", pc.Details.ItemDescription);
    }

    [Fact]
    public async Task Replaces_concession_with_new_amount_using_put_semantics()
    {
        // Arrange
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        package.AddLine(CreditLine.CreateConcession(package.Id, 3000m));

        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        // Act
        var result = await _sut.Handle(
            new UpdatePackageConcessionsCommand(package.PublicId, 7500m), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var credit = Assert.Single(package.Lines.OfType<CreditLine>());
        Assert.Equal(7500m, credit.SalePrice);
        Assert.Equal(0m, credit.EstimatedCost);
        Assert.Equal(0m, credit.RetailSalePrice);
    }

    [Fact]
    public async Task Deletes_existing_concession_when_amount_is_zero()
    {
        // Arrange
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        package.AddLine(CreditLine.CreateConcession(package.Id, 3000m));

        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        // Act
        var result = await _sut.Handle(
            new UpdatePackageConcessionsCommand(package.PublicId, 0m), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(package.Lines.OfType<CreditLine>());
        Assert.Empty(package.Lines.OfType<ProjectCostLine>());
    }

    [Fact]
    public async Task Removes_seller_paid_closing_cost_when_concession_removed()
    {
        // Arrange — simulate existing concession + PC (as if handler had run before)
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        package.AddLine(CreditLine.CreateConcession(package.Id, 5000m));
        package.AddLine(ProjectCostLine.Create(
            packageId: package.Id,
            salePrice: 0m,
            estimatedCost: 5000m,
            retailSalePrice: 0m,
            responsibility: Responsibility.Seller,
            shouldExcludeFromPricing: false,
            details: ProjectCostDetails.Create(14, 1, "Seller Paid Closing Cost")));

        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        // Act
        await _sut.Handle(
            new UpdatePackageConcessionsCommand(package.PublicId, 0m), CancellationToken.None);

        // Assert
        Assert.Empty(package.Lines.OfType<CreditLine>());
        Assert.Empty(package.Lines.OfType<ProjectCostLine>());
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
            new UpdatePackageConcessionsCommand(package.PublicId, 0m), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(package.Lines);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Gross_profit_reduced_by_concession_amount_via_project_cost()
    {
        // Arrange
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);
        var gpBefore = package.GrossProfit;

        // Act
        var result = await _sut.Handle(
            new UpdatePackageConcessionsCommand(package.PublicId, 10000m), CancellationToken.None);

        // Assert — Seller Paid Closing Cost PC (EstimatedCost=10000, SalePrice=0) reduces GP
        Assert.True(result.IsSuccess);
        Assert.Equal(gpBefore - 10000m, result.Value.GrossProfit);
    }

    [Fact]
    public async Task Tax_recalculation_flagged_when_project_cost_added()
    {
        // Arrange — fresh package, no existing lines
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        package.ClearTaxRecalculationFlag(); // Reset so we can detect the flag being set
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        // Act
        await _sut.Handle(
            new UpdatePackageConcessionsCommand(package.PublicId, 5000m), CancellationToken.None);

        // Assert — adding PC changed non-excluded count (0 → 1), so tax flagged
        Assert.True(package.MustRecalculateTaxes);
    }

    [Fact]
    public async Task Tax_recalculation_not_flagged_when_project_cost_count_unchanged()
    {
        // Arrange — existing concession + PC (simulating prior handler run)
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        package.AddLine(CreditLine.CreateConcession(package.Id, 3000m));
        package.AddLine(ProjectCostLine.Create(
            packageId: package.Id,
            salePrice: 0m,
            estimatedCost: 3000m,
            retailSalePrice: 0m,
            responsibility: Responsibility.Seller,
            shouldExcludeFromPricing: false,
            details: ProjectCostDetails.Create(14, 1, "Seller Paid Closing Cost")));
        package.ClearTaxRecalculationFlag();

        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        // Act — update amount (PC replaced, count stays at 1)
        await _sut.Handle(
            new UpdatePackageConcessionsCommand(package.PublicId, 7500m), CancellationToken.None);

        // Assert — non-excluded count unchanged (1 → 1), no tax flag
        Assert.False(package.MustRecalculateTaxes);
    }

    [Fact]
    public async Task Tax_recalculation_flagged_when_project_cost_removed()
    {
        // Arrange — existing concession + PC
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        package.AddLine(CreditLine.CreateConcession(package.Id, 5000m));
        package.AddLine(ProjectCostLine.Create(
            packageId: package.Id,
            salePrice: 0m,
            estimatedCost: 5000m,
            retailSalePrice: 0m,
            responsibility: Responsibility.Seller,
            shouldExcludeFromPricing: false,
            details: ProjectCostDetails.Create(14, 1, "Seller Paid Closing Cost")));
        package.ClearTaxRecalculationFlag();

        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        // Act — remove concession (amount = 0)
        await _sut.Handle(
            new UpdatePackageConcessionsCommand(package.PublicId, 0m), CancellationToken.None);

        // Assert — non-excluded count changed (1 → 0), tax flagged
        Assert.True(package.MustRecalculateTaxes);
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
            new UpdatePackageConcessionsCommand(package.PublicId, 1000m), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(package.GrossProfit, result.Value.GrossProfit);
        Assert.Equal(package.CommissionableGrossProfit, result.Value.CommissionableGrossProfit);
        Assert.Equal(package.MustRecalculateTaxes, result.Value.MustRecalculateTaxes);
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
            new UpdatePackageConcessionsCommand(package.PublicId, 1234.5678m), CancellationToken.None);

        // Assert
        var credit = Assert.Single(package.Lines.OfType<CreditLine>());
        Assert.Equal(1234.57m, credit.SalePrice);
    }

    [Fact]
    public async Task Updates_seller_paid_closing_cost_amount_when_concession_changes()
    {
        // Arrange — existing concession + PC
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        package.AddLine(CreditLine.CreateConcession(package.Id, 3000m));
        package.AddLine(ProjectCostLine.Create(
            packageId: package.Id,
            salePrice: 0m,
            estimatedCost: 3000m,
            retailSalePrice: 0m,
            responsibility: Responsibility.Seller,
            shouldExcludeFromPricing: false,
            details: ProjectCostDetails.Create(14, 1, "Seller Paid Closing Cost")));

        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        // Act — change concession from 3000 to 8000
        await _sut.Handle(
            new UpdatePackageConcessionsCommand(package.PublicId, 8000m), CancellationToken.None);

        // Assert — PC updated to new amount
        var pc = Assert.Single(package.Lines.OfType<ProjectCostLine>());
        Assert.Equal(8000m, pc.EstimatedCost);
        Assert.Equal(0m, pc.SalePrice);
    }
}
