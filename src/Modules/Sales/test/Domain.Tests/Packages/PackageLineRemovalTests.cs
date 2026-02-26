using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Details;
using Modules.Sales.Domain.Packages.Events;
using Modules.Sales.Domain.Packages.Lines;
using Xunit;

namespace Modules.Sales.Domain.Tests.Packages;

public sealed class PackageLineRemovalTests
{
    // --- 1:1 removals ---

    [Fact]
    public void RemoveHomeLine_removes_existing_home_line()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);
        package.AddLine(HomeLine.Create(package.Id, 100_000m, 80_000m, 100_000m, Responsibility.Seller, details: null));

        var removed = package.RemoveLine<HomeLine>();

        Assert.NotNull(removed);
        Assert.Empty(package.Lines.OfType<HomeLine>());
    }

    [Fact]
    public void RemoveHomeLine_returns_null_when_absent()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);

        var removed = package.RemoveLine<HomeLine>();

        Assert.Null(removed);
    }

    [Fact]
    public void RemoveLandLine_removes_existing_land_line()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);
        package.AddLine(LandLine.Create(package.Id, 50_000m, 40_000m, 50_000m, Responsibility.Seller, details: null));

        var removed = package.RemoveLine<LandLine>();

        Assert.NotNull(removed);
        Assert.Empty(package.Lines.OfType<LandLine>());
    }

    [Fact]
    public void RemoveLandLine_returns_null_when_absent()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);

        Assert.Null(package.RemoveLine<LandLine>());
    }

    [Fact]
    public void RemoveTaxLine_removes_existing_tax_line()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);
        package.AddLine(TaxLine.Create(package.Id, 500m, 0m, 0m, shouldExcludeFromPricing: false, details: null));

        var removed = package.RemoveLine<TaxLine>();

        Assert.NotNull(removed);
        Assert.Empty(package.Lines.OfType<TaxLine>());
    }

    [Fact]
    public void RemoveTaxLine_returns_null_when_absent()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);

        Assert.Null(package.RemoveLine<TaxLine>());
    }

    [Fact]
    public void RemoveWarrantyLine_removes_existing_warranty_line()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);
        var details = WarrantyDetails.Create(875m, 72.19m);
        package.AddLine(WarrantyLine.Create(package.Id, 875m, 0m, 0m, shouldExcludeFromPricing: false, details: details));

        var removed = package.RemoveLine<WarrantyLine>();

        Assert.NotNull(removed);
        Assert.Empty(package.Lines.OfType<WarrantyLine>());
    }

    [Fact]
    public void RemoveWarrantyLine_returns_null_when_absent()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);

        Assert.Null(package.RemoveLine<WarrantyLine>());
    }

    [Fact]
    public void RemoveSalesTeamLine_removes_existing_sales_team_line()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);
        package.AddLine(SalesTeamLine.Create(package.Id, details: null));

        var removed = package.RemoveLine<SalesTeamLine>();

        Assert.NotNull(removed);
        Assert.Empty(package.Lines.OfType<SalesTeamLine>());
    }

    [Fact]
    public void RemoveSalesTeamLine_returns_null_when_absent()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);

        Assert.Null(package.RemoveLine<SalesTeamLine>());
    }

    // --- 1:1 removal recalculates gross profit ---

    [Fact]
    public void RemoveWarrantyLine_recalculates_gross_profit_when_caller_recalculates()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);
        var details = WarrantyDetails.Create(875m, 0m);
        package.AddLine(WarrantyLine.Create(package.Id, 875m, 0m, 0m, shouldExcludeFromPricing: false, details: details));
        package.RecalculateGrossProfit();
        var gpWithWarranty = package.GrossProfit;

        package.RemoveLine<WarrantyLine>();
        package.RecalculateGrossProfit();

        Assert.NotEqual(gpWithWarranty, package.GrossProfit);
    }

    // --- Insurance ---

    [Fact]
    public void RemoveOutsideInsuranceLine_removes_outside_insurance()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);
        var details = InsuranceDetails.Create(InsuranceType.Outside, 100_000m, providerName: "StateFarm", totalPremium: 300m);
        package.AddLine(InsuranceLine.Create(package.Id, 300m, 0m, 0m, Responsibility.Buyer, shouldExcludeFromPricing: false, details: details));

        var removed = package.RemoveOutsideInsuranceLine();

        Assert.NotNull(removed);
        Assert.Equal(InsuranceType.Outside, removed.Details!.InsuranceType);
    }

    [Fact]
    public void RemoveOutsideInsuranceLine_returns_null_when_absent()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);

        Assert.Null(package.RemoveOutsideInsuranceLine());
    }

    [Fact]
    public void RemoveOutsideInsuranceLine_leaves_HomeFirst_intact()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);

        var homeFirst = InsuranceDetails.Create(InsuranceType.HomeFirst, 100_000m, totalPremium: 250m);
        package.AddLine(InsuranceLine.Create(package.Id, 250m, 0m, 0m, Responsibility.Buyer, shouldExcludeFromPricing: false, details: homeFirst));

        var outside = InsuranceDetails.Create(InsuranceType.Outside, 100_000m, providerName: "StateFarm", totalPremium: 300m);
        package.AddLine(InsuranceLine.Create(package.Id, 300m, 0m, 0m, Responsibility.Buyer, shouldExcludeFromPricing: false, details: outside));

        var removed = package.RemoveOutsideInsuranceLine();

        Assert.NotNull(removed);
        Assert.Equal(InsuranceType.Outside, removed.Details!.InsuranceType);
        Assert.Single(package.Lines.OfType<InsuranceLine>());
        Assert.Equal(InsuranceType.HomeFirst, package.Lines.OfType<InsuranceLine>().Single().Details!.InsuranceType);
    }

    [Fact]
    public void RemoveHomeFirstInsuranceLine_removes_only_HomeFirst()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);

        var homeFirst = InsuranceDetails.Create(InsuranceType.HomeFirst, 100_000m, totalPremium: 250m);
        package.AddLine(InsuranceLine.Create(package.Id, 250m, 0m, 0m, Responsibility.Buyer, shouldExcludeFromPricing: false, details: homeFirst));

        var outside = InsuranceDetails.Create(InsuranceType.Outside, 100_000m, providerName: "StateFarm", totalPremium: 300m);
        package.AddLine(InsuranceLine.Create(package.Id, 300m, 0m, 0m, Responsibility.Buyer, shouldExcludeFromPricing: false, details: outside));

        var removed = package.RemoveHomeFirstInsuranceLine();

        Assert.NotNull(removed);
        Assert.Equal(InsuranceType.HomeFirst, removed.Details!.InsuranceType);
        var remaining = Assert.Single(package.Lines.OfType<InsuranceLine>());
        Assert.Equal(InsuranceType.Outside, remaining.Details!.InsuranceType);
    }

    [Fact]
    public void RemoveHomeFirstInsuranceLine_returns_null_when_no_HomeFirst()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);
        var outside = InsuranceDetails.Create(InsuranceType.Outside, 100_000m, providerName: "StateFarm", totalPremium: 300m);
        package.AddLine(InsuranceLine.Create(package.Id, 300m, 0m, 0m, Responsibility.Buyer, shouldExcludeFromPricing: false, details: outside));

        var removed = package.RemoveHomeFirstInsuranceLine();

        Assert.Null(removed);
        Assert.Single(package.Lines.OfType<InsuranceLine>());
    }

    // --- Credits ---

    [Fact]
    public void RemoveDownPaymentLine_removes_down_payment_leaves_concession()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);
        package.AddLine(CreditLine.CreateDownPayment(package.Id, 5000m));
        package.AddLine(CreditLine.CreateConcession(package.Id, 2000m));

        var removed = package.RemoveDownPaymentLine();

        Assert.NotNull(removed);
        Assert.True(removed.IsDownPayment);
        var remaining = Assert.Single(package.Lines.OfType<CreditLine>());
        Assert.True(remaining.IsConcession);
    }

    [Fact]
    public void RemoveConcessionLine_removes_concession_leaves_down_payment()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);
        package.AddLine(CreditLine.CreateDownPayment(package.Id, 5000m));
        package.AddLine(CreditLine.CreateConcession(package.Id, 2000m));

        var removed = package.RemoveConcessionLine();

        Assert.NotNull(removed);
        Assert.True(removed.IsConcession);
        var remaining = Assert.Single(package.Lines.OfType<CreditLine>());
        Assert.True(remaining.IsDownPayment);
    }

    [Fact]
    public void RemoveDownPaymentLine_returns_null_when_absent()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);

        Assert.Null(package.RemoveDownPaymentLine());
    }

    [Fact]
    public void RemoveConcessionLine_returns_null_when_absent()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);

        Assert.Null(package.RemoveConcessionLine());
    }

    // --- Project costs ---

    [Fact]
    public void RemoveProjectCost_removes_by_key()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);
        var details = ProjectCostDetails.Create(9, 21, "Use Tax");
        package.AddLine(ProjectCostLine.Create(package.Id, 100m, 100m, 100m, Responsibility.Seller, shouldExcludeFromPricing: false, details: details));

        var removed = package.RemoveProjectCost(9, 21);

        Assert.NotNull(removed);
        Assert.Empty(package.Lines.OfType<ProjectCostLine>());
    }

    [Fact]
    public void RemoveProjectCost_returns_null_when_key_not_found()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);
        var details = ProjectCostDetails.Create(9, 21, "Use Tax");
        package.AddLine(ProjectCostLine.Create(package.Id, 100m, 100m, 100m, Responsibility.Seller, shouldExcludeFromPricing: false, details: details));

        var removed = package.RemoveProjectCost(1, 28);

        Assert.Null(removed);
        Assert.Single(package.Lines.OfType<ProjectCostLine>());
    }

    [Fact]
    public void RemoveProjectCost_recalculates_gross_profit_when_caller_recalculates()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);
        var details = ProjectCostDetails.Create(9, 21, "Use Tax");
        package.AddLine(ProjectCostLine.Create(package.Id, 100m, 50m, 100m, Responsibility.Seller, shouldExcludeFromPricing: false, details: details));
        package.RecalculateGrossProfit();
        var gpBefore = package.GrossProfit;

        package.RemoveProjectCost(9, 21);
        package.RecalculateGrossProfit();

        Assert.NotEqual(gpBefore, package.GrossProfit);
    }

    [Fact]
    public void RemoveProjectCostsByCategory_removes_all_in_category()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);
        package.AddLine(ProjectCostLine.Create(package.Id, 100m, 100m, 100m, Responsibility.Seller, shouldExcludeFromPricing: false, details: ProjectCostDetails.Create(12, 1, "Repo cost A")));
        package.AddLine(ProjectCostLine.Create(package.Id, 200m, 200m, 200m, Responsibility.Seller, shouldExcludeFromPricing: false, details: ProjectCostDetails.Create(12, 2, "Repo cost B")));
        package.AddLine(ProjectCostLine.Create(package.Id, 50m, 50m, 50m, Responsibility.Seller, shouldExcludeFromPricing: false, details: ProjectCostDetails.Create(9, 21, "Use Tax")));

        var removed = package.RemoveProjectCostsByCategory(12);

        Assert.Equal(2, removed);
        Assert.DoesNotContain(package.Lines.OfType<ProjectCostLine>(), l => l.Details?.CategoryId == 12);
        Assert.Single(package.Lines.OfType<ProjectCostLine>(), l => l.Details?.CategoryId == 9);
    }

    [Fact]
    public void RemoveProjectCostsByCategory_returns_zero_when_no_match()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);

        var removed = package.RemoveProjectCostsByCategory(99);

        Assert.Equal(0, removed);
    }

    [Fact]
    public void RemoveAllProjectCosts_removes_all_matching_key()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);
        package.AddLine(ProjectCostLine.Create(package.Id, 100m, 100m, 100m, Responsibility.Seller, shouldExcludeFromPricing: false, details: ProjectCostDetails.Create(10, 9, "Trade Over A")));
        package.AddLine(ProjectCostLine.Create(package.Id, 200m, 200m, 200m, Responsibility.Seller, shouldExcludeFromPricing: false, details: ProjectCostDetails.Create(10, 9, "Trade Over B")));
        package.AddLine(ProjectCostLine.Create(package.Id, 50m, 50m, 50m, Responsibility.Seller, shouldExcludeFromPricing: false, details: ProjectCostDetails.Create(9, 21, "Use Tax")));

        var removed = package.RemoveAllProjectCosts(10, 9);

        Assert.Equal(2, removed);
        Assert.DoesNotContain(package.Lines.OfType<ProjectCostLine>(), l => l.Details?.CategoryId == 10);
        Assert.Single(package.Lines.OfType<ProjectCostLine>(), l => l.Details?.CategoryId == 9);
    }

    [Fact]
    public void RemoveAllProjectCosts_returns_zero_when_no_match()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);

        var removed = package.RemoveAllProjectCosts(99, 99);

        Assert.Equal(0, removed);
    }

    // --- W-12: Domain events on line removal ---

    [Fact]
    public void RemoveHomeLine_raises_HomeLineUpdatedDomainEvent()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);
        package.AddLine(HomeLine.Create(package.Id, 100_000m, 80_000m, 100_000m, Responsibility.Seller, details: null));
        package.ClearDomainEvents();

        package.RemoveLine<HomeLine>();

        Assert.Contains(package.DomainEvents, e => e is HomeLineUpdatedDomainEvent);
    }

    [Fact]
    public void RemoveHomeLine_does_not_raise_event_when_absent()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);
        package.ClearDomainEvents();

        package.RemoveLine<HomeLine>();

        Assert.DoesNotContain(package.DomainEvents, e => e is HomeLineUpdatedDomainEvent);
    }

    [Fact]
    public void RemoveLandLine_raises_LandLineUpdatedDomainEvent()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);
        package.AddLine(LandLine.Create(package.Id, 50_000m, 40_000m, 50_000m, Responsibility.Seller, details: null));
        package.ClearDomainEvents();

        package.RemoveLine<LandLine>();

        Assert.Contains(package.DomainEvents, e => e is LandLineUpdatedDomainEvent);
    }

    [Fact]
    public void RemoveLandLine_does_not_raise_event_when_absent()
    {
        var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);
        package.ClearDomainEvents();

        package.RemoveLine<LandLine>();

        Assert.DoesNotContain(package.DomainEvents, e => e is LandLineUpdatedDomainEvent);
    }

}
