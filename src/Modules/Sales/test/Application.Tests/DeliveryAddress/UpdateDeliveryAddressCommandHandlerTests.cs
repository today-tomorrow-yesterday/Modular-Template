using Modules.Sales.Application.DeliveryAddresses.UpdateDeliveryAddress;
using Modules.Sales.Domain;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Insurance;
using Modules.Sales.Domain.Packages.ProjectCosts;
using Modules.Sales.Domain.Packages.Tax;
using Modules.Sales.Domain.Packages.Warranty;
using Modules.Sales.Domain.Sales;
using NSubstitute;
using Rtl.Core.Application.Persistence;
using System.Reflection;
using Xunit;

namespace Modules.Sales.Application.Tests.DeliveryAddress;

public sealed class UpdateDeliveryAddressCommandHandlerTests
{
    private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
    private readonly IPackageRepository _packageRepository = Substitute.For<IPackageRepository>();
    private readonly IUnitOfWork<ISalesModule> _unitOfWork = Substitute.For<IUnitOfWork<ISalesModule>>();
    private readonly UpdateDeliveryAddressCommandHandler _sut;

    public UpdateDeliveryAddressCommandHandlerTests() =>
        _sut = new UpdateDeliveryAddressCommandHandler(_saleRepository, _packageRepository, _unitOfWork);

    [Fact]
    public async Task Returns_failure_when_sale_not_found()
    {
        var publicId = Guid.NewGuid();
        _saleRepository.GetByPublicIdWithDeliveryAddressAsync(publicId, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);

        var result = await _sut.Handle(
            CreateCommand(publicId),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Sale.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Returns_failure_when_delivery_address_not_found()
    {
        var sale = Sale.Create(customerId: 1, retailLocationId: 1, saleType: SaleType.B2C);
        sale.ClearDomainEvents();
        // Sale has no DeliveryAddress (null by default)

        _saleRepository.GetByPublicIdWithDeliveryAddressAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);

        var result = await _sut.Handle(
            CreateCommand(sale.PublicId),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("DeliveryAddress.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Successful_update_returns_success()
    {
        var sale = CreateSaleWithDeliveryAddress();

        _saleRepository.GetByPublicIdWithDeliveryAddressAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);

        var result = await _sut.Handle(
            CreateCommand(sale.PublicId),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Calls_save_changes()
    {
        var sale = CreateSaleWithDeliveryAddress();

        _saleRepository.GetByPublicIdWithDeliveryAddressAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);

        await _sut.Handle(
            CreateCommand(sale.PublicId),
            CancellationToken.None);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Does_not_load_packages_when_no_relevant_changes()
    {
        // Address: OH, Columbus, Franklin, 43004, Primary Residence, withinCityLimits=true
        // Command: OH, Columbus, Franklin, 43004, Primary Residence, withinCityLimits=true (same values)
        var sale = CreateSaleWithDeliveryAddress();

        _saleRepository.GetByPublicIdWithDeliveryAddressAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);

        var command = new UpdateDeliveryAddressCommand(
            sale.PublicId, "Primary Residence", true,
            "999 New St", null, "Columbus", "Franklin", "OH", "43004");

        await _sut.Handle(command, CancellationToken.None);

        await _packageRepository.DidNotReceive()
            .GetBySaleIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task State_change_clears_tax_question_answers_and_flags_recalculation()
    {
        var sale = CreateSaleWithDeliveryAddress();
        var package = CreateDraftPackageWithTaxLine(sale.Id);

        _saleRepository.GetByPublicIdWithDeliveryAddressAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);
        _packageRepository.GetBySaleIdWithTrackingAsync(sale.Id, Arg.Any<CancellationToken>())
            .Returns(new[] { package });

        // Change state from OH to TX
        var command = new UpdateDeliveryAddressCommand(
            sale.PublicId, "Primary Residence", true,
            "999 New St", null, "Columbus", "Franklin", "TX", "43004");

        await _sut.Handle(command, CancellationToken.None);

        var taxLine = package.Lines.OfType<TaxLine>().Single();
        // Q&A should be cleared, but calculations should remain
        Assert.Empty(taxLine.Details!.StateTaxQuestionAnswers);
        Assert.True(package.MustRecalculateTaxes);
    }

    [Fact]
    public async Task Occupancy_became_ineligible_removes_insurance_and_warranty_from_all_packages()
    {
        var sale = CreateSaleWithDeliveryAddress();
        var package = CreateDraftPackageWithInsuranceAndWarranty(sale.Id);

        _saleRepository.GetByPublicIdWithDeliveryAddressAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);
        _packageRepository.GetBySaleIdWithTrackingAsync(sale.Id, Arg.Any<CancellationToken>())
            .Returns(new[] { package });

        // Change occupancy to Rental (insurance-ineligible)
        var command = new UpdateDeliveryAddressCommand(
            sale.PublicId, "Rental", true,
            "999 New St", null, "Columbus", "Franklin", "OH", "43004");

        await _sut.Handle(command, CancellationToken.None);

        Assert.DoesNotContain(package.Lines.OfType<InsuranceLine>(),
            l => l.Details?.InsuranceType == InsuranceType.HomeFirst);
        Assert.DoesNotContain(package.Lines, l => l is WarrantyLine);
    }

    [Fact]
    public async Task Location_change_clears_tax_calculations_and_removes_use_tax_on_draft_packages()
    {
        var sale = CreateSaleWithDeliveryAddress();
        var package = CreateDraftPackageWithTaxLineAndUseTax(sale.Id);

        _saleRepository.GetByPublicIdWithDeliveryAddressAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);
        _packageRepository.GetBySaleIdWithTrackingAsync(sale.Id, Arg.Any<CancellationToken>())
            .Returns(new[] { package });

        // Change city from Columbus to Dublin (location change)
        var command = new UpdateDeliveryAddressCommand(
            sale.PublicId, "Primary Residence", true,
            "999 New St", null, "Dublin", "Franklin", "OH", "43017");

        await _sut.Handle(command, CancellationToken.None);

        var taxLine = package.Lines.OfType<TaxLine>().Single();
        // Tax calculations should be cleared
        Assert.Empty(taxLine.Details!.Taxes);
        // Use Tax project cost (Cat 9/21) should be removed
        Assert.Empty(package.Lines.OfType<ProjectCostLine>());
        Assert.True(package.MustRecalculateTaxes);
    }

    // -- Helpers --

    private static UpdateDeliveryAddressCommand CreateCommand(Guid salePublicId) =>
        new(salePublicId, "Secondary Residence", false,
            "999 New St", null, "Dublin", "Franklin", "OH", "43017");

    private static Sale CreateSaleWithDeliveryAddress()
    {
        var sale = Sale.Create(customerId: 1, retailLocationId: 1, saleType: SaleType.B2C);
        sale.ClearDomainEvents();

        var address = Domain.DeliveryAddresses.DeliveryAddress.Create(
            sale.Id, "Primary Residence", true, null, null, "123 Main", null, null, "Columbus", "Franklin", "OH", "US", "43004");
        address.ClearDomainEvents();
        SetProperty(sale, nameof(Sale.DeliveryAddress), address);

        return sale;
    }

    private static Package CreateDraftPackageWithTaxLine(int saleId)
    {
        var package = Package.Create(saleId: saleId, name: "Primary", isPrimary: true);
        package.ClearDomainEvents();

        var taxDetails = TaxDetails.Create(
            previouslyTitled: null,
            taxExemptionId: null,
            questionAnswers: [TaxQuestionAnswer.Create(1, "Yes", "Is this taxable?")],
            taxes: [TaxItem.Create("State Tax", 50m)],
            errors: null);

        package.AddLine(TaxLine.Create(package.Id, 500m, 0m, 0m, shouldExcludeFromPricing: false, details: taxDetails));
        package.ClearDomainEvents();
        package.ClearTaxRecalculationFlag();

        return package;
    }

    private static Package CreateDraftPackageWithInsuranceAndWarranty(int saleId)
    {
        var package = Package.Create(saleId: saleId, name: "Primary", isPrimary: true);
        package.ClearDomainEvents();

        var insuranceDetails = InsuranceDetails.Create(InsuranceType.HomeFirst, 100_000m, totalPremium: 250m);
        package.AddLine(InsuranceLine.Create(
            package.Id, 250m, 0m, 0m, Responsibility.Buyer,
            shouldExcludeFromPricing: false, details: insuranceDetails));

        var warrantyDetails = WarrantyDetails.Create(875m, 72.19m);
        package.AddLine(WarrantyLine.Create(
            package.Id, 875m, 0m, 0m,
            shouldExcludeFromPricing: false, details: warrantyDetails));

        package.ClearDomainEvents();

        return package;
    }

    private static Package CreateDraftPackageWithTaxLineAndUseTax(int saleId)
    {
        var package = Package.Create(saleId: saleId, name: "Primary", isPrimary: true);
        package.ClearDomainEvents();

        var taxDetails = TaxDetails.Create(
            previouslyTitled: null,
            taxExemptionId: null,
            questionAnswers: [],
            taxes: [TaxItem.Create("County Tax", 25m)],
            errors: null);

        package.AddLine(TaxLine.Create(package.Id, 500m, 0m, 0m, shouldExcludeFromPricing: false, details: taxDetails));

        var pcDetails = ProjectCostDetails.Create(9, 21, "Use Tax");
        package.AddLine(ProjectCostLine.Create(
            package.Id, 100m, 100m, 100m, Responsibility.Seller,
            shouldExcludeFromPricing: false, details: pcDetails));

        package.ClearDomainEvents();
        package.ClearTaxRecalculationFlag();

        return package;
    }

    private static void SetProperty(object obj, string propertyName, object? value)
    {
        var backingField = obj.GetType().GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
        if (backingField is not null)
        {
            backingField.SetValue(obj, value);
        }
        else
        {
            obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)!.SetValue(obj, value);
        }
    }
}
