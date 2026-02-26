using Modules.Sales.Application.Insurance.RecordOutsideInsurance;
using Modules.Sales.Domain;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Insurance;
using Modules.Sales.Domain.RetailLocations;
using Modules.Sales.Domain.Sales;
using NSubstitute;
using Rtl.Core.Application.Persistence;
using System.Reflection;
using Xunit;

namespace Modules.Sales.Application.Tests.Insurance;

public sealed class RecordOutsideInsuranceCommandHandlerTests
{
    private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
    private readonly IUnitOfWork<ISalesModule> _unitOfWork = Substitute.For<IUnitOfWork<ISalesModule>>();
    private readonly RecordOutsideInsuranceCommandHandler _sut;

    public RecordOutsideInsuranceCommandHandlerTests() =>
        _sut = new RecordOutsideInsuranceCommandHandler(_saleRepository, _unitOfWork);

    [Fact]
    public async Task Returns_failure_when_sale_not_found()
    {
        var publicId = Guid.NewGuid();
        _saleRepository.GetByPublicIdWithFullContextAsync(publicId, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);

        var result = await _sut.Handle(
            new RecordOutsideInsuranceCommand(publicId, "Acme Insurance", 100000m, 1200m),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Sale.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Returns_failure_when_no_primary_package()
    {
        var sale = CreateSaleWithContext(includePrimaryPackage: false);
        _saleRepository.GetByPublicIdWithFullContextAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);

        var result = await _sut.Handle(
            new RecordOutsideInsuranceCommand(sale.PublicId, "Acme Insurance", 100000m, 1200m),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Package.NoPrimaryPackage", result.Error.Code);
    }

    [Fact]
    public async Task Returns_success_and_creates_insurance_line()
    {
        var sale = CreateSaleWithContext();
        _saleRepository.GetByPublicIdWithFullContextAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);

        var result = await _sut.Handle(
            new RecordOutsideInsuranceCommand(sale.PublicId, "Acme Insurance", 100000m, 1200m),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var package = sale.Packages.First(p => p.IsPrimaryPackage);
        var insuranceLine = Assert.Single(package.Lines.OfType<InsuranceLine>());
        Assert.Equal(1200m, insuranceLine.SalePrice);
        Assert.Equal(InsuranceType.Outside, insuranceLine.Details!.InsuranceType);
    }

    // --- Test helpers ---

    private static Sale CreateSaleWithContext(bool includePrimaryPackage = true)
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

        if (includePrimaryPackage)
        {
            var package = Package.Create(saleId: sale.Id, name: "Primary", isPrimary: true);
            package.ClearDomainEvents();

            var packagesField = typeof(Sale).GetField("_packages", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var packages = (List<Package>)packagesField.GetValue(sale)!;
            packages.Add(package);
        }

        return sale;
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
