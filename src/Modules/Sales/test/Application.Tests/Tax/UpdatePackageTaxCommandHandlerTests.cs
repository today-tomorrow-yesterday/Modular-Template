using Modules.Sales.Application.Tax.UpdatePackageTax;
using Modules.Sales.Domain;
using Modules.Sales.Domain.Cdc;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.RetailLocations;
using Modules.Sales.Domain.Sales;
using NSubstitute;
using Rtl.Core.Application.Persistence;
using System.Reflection;
using Xunit;

namespace Modules.Sales.Application.Tests.Tax;

public sealed class UpdatePackageTaxCommandHandlerTests
{
    private readonly IPackageRepository _packageRepository = Substitute.For<IPackageRepository>();
    private readonly ICdcTaxQueries _cdcTaxQueries = Substitute.For<ICdcTaxQueries>();
    private readonly IUnitOfWork<ISalesModule> _unitOfWork = Substitute.For<IUnitOfWork<ISalesModule>>();
    private readonly UpdatePackageTaxCommandHandler _sut;

    public UpdatePackageTaxCommandHandlerTests() =>
        _sut = new UpdatePackageTaxCommandHandler(_packageRepository, _cdcTaxQueries, _unitOfWork);

    [Fact]
    public async Task Returns_failure_when_package_not_found()
    {
        var publicId = Guid.NewGuid();
        _packageRepository.GetByPublicIdWithSaleContextAsync(publicId, Arg.Any<CancellationToken>())
            .Returns((Package?)null);

        var result = await _sut.Handle(
            new UpdatePackageTaxCommand(publicId, "Yes", null, []),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Package.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Returns_success_and_flags_tax_recalculation()
    {
        var package = CreatePackageWithContext();
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);
        _cdcTaxQueries.GetQuestionTextsByNumbersAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<int, string> { { 1, "Is the home new?" } });

        var questionAnswers = new List<TaxQuestionAnswerRequest> { new(1, true) };
        var result = await _sut.Handle(
            new UpdatePackageTaxCommand(package.PublicId, "Yes", null, questionAnswers),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.MustRecalculateTaxes);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // --- Test helpers ---

    private static Package CreatePackageWithContext()
    {
        var sale = Sale.Create(
            partyId: 1,
            retailLocationId: 1,
            saleType: SaleType.B2C,
            saleNumber: 12345);
        sale.ClearDomainEvents();

        var retailLocation = RetailLocation.CreateHomeCenter(
            homeCenterNumber: 42, name: "Test HC", stateCode: "OH", zip: "43004", isActive: true);
        SetProperty(sale, nameof(Sale.RetailLocation), retailLocation);

        var package = Package.Create(saleId: sale.Id, name: "Primary", isPrimary: true);
        package.ClearDomainEvents();

        SetProperty(package, nameof(Package.Sale), sale);

        var packagesField = typeof(Sale).GetField("_packages", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var packages = (List<Package>)packagesField.GetValue(sale)!;
        packages.Add(package);

        return package;
    }

    private static void SetProperty(object obj, string propertyName, object? value)
    {
        var backingField = obj.GetType().GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
        if (backingField is not null)
            backingField.SetValue(obj, value);
        else
            obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)!.SetValue(obj, value);
    }
}
