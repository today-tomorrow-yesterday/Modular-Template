using Modules.Sales.Application.Packages.GetPackageById;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Credits;
using Modules.Sales.Domain.Packages.Home;
using Modules.Sales.Domain.Packages.Insurance;
using Modules.Sales.Domain.Packages.Land;
using Modules.Sales.Domain.Packages.ProjectCosts;
using Modules.Sales.Domain.Packages.SalesTeam;
using Modules.Sales.Domain.Packages.Tax;
using Modules.Sales.Domain.Packages.TradeIns;
using Modules.Sales.Domain.Packages.Warranty;
using Xunit;

namespace Modules.Sales.Application.Tests.Packages;

public sealed class PackageDetailMapperTests
{
    // ─── Package header ───────────────────────────────────────────────

    [Fact]
    public void Maps_package_header_fields()
    {
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);

        var response = PackageDetailMapper.MapToResponse(package);

        Assert.Equal(package.PublicId, response.Id);
        Assert.Equal("Primary", response.Name);
        Assert.Equal(1, response.Ranking);
        Assert.True(response.IsPrimaryPackage);
        Assert.Equal("Draft", response.Status);
        Assert.Equal(0m, response.GrossProfit);
        Assert.Equal(0m, response.CommissionableGrossProfit);
        Assert.True(response.MustRecalculateTaxes);
        Assert.Empty(response.FundingRequestIds);
        Assert.Null(response.Funding);
    }

    [Fact]
    public void Maps_non_primary_package()
    {
        var package = Package.Create(saleId: 1, name: "Alternate", isPrimary: false);

        var response = PackageDetailMapper.MapToResponse(package);

        Assert.Equal(2, response.Ranking);
        Assert.False(response.IsPrimaryPackage);
    }

    // ─── Empty package — all sections null/empty ──────────────────────

    [Fact]
    public void Empty_package_has_all_sections_null_or_empty()
    {
        var package = Package.Create(saleId: 1, name: "Empty", isPrimary: true);

        var response = PackageDetailMapper.MapToResponse(package);

        Assert.Null(response.Home);
        Assert.Null(response.Land);
        Assert.Null(response.Tax);
        Assert.Null(response.Insurance);
        Assert.Null(response.Warranty);
        Assert.Null(response.DownPayment);
        Assert.Null(response.Concessions);
        Assert.Empty(response.TradeIns);
        Assert.Empty(response.SalesTeam);
        Assert.Empty(response.ProjectCosts);
    }

    // ─── Home section ─────────────────────────────────────────────────

    [Fact]
    public void Maps_home_section_with_populated_details()
    {
        var package = Package.Create(saleId: 1, name: "P", isPrimary: true);
        var details = HomeDetails.Create(
            homeType: HomeType.New,
            homeSourceType: HomeSourceType.OnLot,
            stockNumber: "STK-001",
            make: "Clayton",
            model: "Pegasus",
            modelYear: 2026,
            lengthInFeet: 76m,
            widthInFeet: 16m,
            bedrooms: 3,
            bathrooms: 2m,
            squareFootage: "1216",
            numberOfFloorSections: 1,
            modularType: ModularType.Hud,
            buildType: "SingleWide",
            claytonBuilt: true,
            vendor: "CMH",
            baseCost: 40000m,
            optionsCost: 5000m,
            freightCost: 3000m,
            invoiceCost: 48000m,
            netInvoice: 47000m,
            grossCost: 50000m,
            taxIncludedOnInvoice: 500m,
            rebateOnMfgInvoice: 200m,
            stateAssociationAndMhiDues: 100m,
            partnerAssistance: 300m,
            numberOfWheels: 6,
            numberOfAxles: 3,
            wheelAndAxlesOption: WheelAndAxlesOption.Rent,
            carrierFrameDeposit: 150m,
            distanceMiles: 125.5,
            propertyType: "Residential",
            purchaseOption: "Standard",
            listingPrice: 85000m,
            accountNumber: "ACC-001",
            displayAccountId: "DISP-001",
            streetAddress: "123 Main St",
            city: "Dallas",
            state: "TX",
            zipCode: "75201",
            inventoryReferenceId: "INV-001",
            serialNumbers: ["SN-A", "SN-B"]);

        package.AddLine(HomeLine.Create(package.Id, 80000m, 50000m, 85000m, Responsibility.Buyer, details));

        var response = PackageDetailMapper.MapToResponse(package);

        Assert.NotNull(response.Home);
        var h = response.Home!;
        Assert.Equal(80000m, h.SalePrice);
        Assert.Equal(50000m, h.EstimatedCost);
        Assert.Equal(85000m, h.RetailSalePrice);
        Assert.Equal("Buyer", h.Responsibility);
        Assert.False(h.ShouldExcludeFromPricing);
        Assert.Equal("New", h.HomeType);
        Assert.Equal("OnLot", h.HomeSourceType);
        Assert.Equal("STK-001", h.StockNumber);
        Assert.Equal("Clayton", h.Make);
        Assert.Equal("Pegasus", h.Model);
        Assert.Equal(2026, h.ModelYear);
        Assert.Equal(76m, h.LengthInFeet);
        Assert.Equal(16m, h.WidthInFeet);
        Assert.Equal(3, h.Bedrooms);
        Assert.Equal(2m, h.Bathrooms);
        Assert.Equal("1216", h.SquareFootage);
        Assert.Equal(1, h.NumberOfFloorSections);
        Assert.Equal("Hud", h.ModularType);
        Assert.Equal("SingleWide", h.BuildType);
        Assert.True(h.ClaytonBuilt);
        Assert.Equal("CMH", h.Vendor);
        Assert.Equal(40000m, h.BaseCost);
        Assert.Equal(5000m, h.OptionsCost);
        Assert.Equal(3000m, h.FreightCost);
        Assert.Equal(48000m, h.InvoiceCost);
        Assert.Equal(47000m, h.NetInvoice);
        Assert.Equal(50000m, h.GrossCost);
        Assert.Equal(500m, h.TaxIncludedOnInvoice);
        Assert.Equal(200m, h.RebateOnMfgInvoice);
        Assert.Equal(100m, h.StateAssociationAndMhiDues);
        Assert.Equal(300m, h.PartnerAssistance);
        Assert.Equal(6, h.NumberOfWheels);
        Assert.Equal(3, h.NumberOfAxles);
        Assert.Equal("Rent", h.WheelAndAxlesOption);
        Assert.Equal(150m, h.CarrierFrameDeposit);
        Assert.Equal(125.5, h.DistanceMiles);
        Assert.Equal("Residential", h.PropertyType);
        Assert.Equal("Standard", h.PurchaseOption);
        Assert.Equal(85000m, h.ListingPrice);
        Assert.Equal("ACC-001", h.AccountNumber);
        Assert.Equal("DISP-001", h.DisplayAccountId);
        Assert.Equal("123 Main St", h.StreetAddress);
        Assert.Equal("Dallas", h.City);
        Assert.Equal("TX", h.State);
        Assert.Equal("75201", h.ZipCode);
        Assert.Equal("INV-001", h.InventoryReferenceId);
        Assert.NotNull(h.SerialNumbers);
        Assert.Equal(["SN-A", "SN-B"], h.SerialNumbers!);
    }

    [Fact]
    public void Maps_home_section_with_null_details()
    {
        var package = Package.Create(saleId: 1, name: "P", isPrimary: true);
        package.AddLine(HomeLine.Create(package.Id, 80000m, 50000m, 85000m, null, details: null));

        var response = PackageDetailMapper.MapToResponse(package);

        Assert.NotNull(response.Home);
        Assert.Equal(80000m, response.Home!.SalePrice);
        Assert.Null(response.Home.Responsibility);
        Assert.Null(response.Home.HomeType);
        Assert.Null(response.Home.StockNumber);
        Assert.Null(response.Home.Make);
    }

    // ─── Land section ─────────────────────────────────────────────────

    [Fact]
    public void Maps_land_section_with_null_details_returns_fallback()
    {
        var package = Package.Create(saleId: 1, name: "P", isPrimary: true);
        package.AddLine(LandLine.Create(package.Id, 25000m, 15000m, 30000m, null, details: null));

        var response = PackageDetailMapper.MapToResponse(package);

        Assert.NotNull(response.Land);
        Assert.Equal(25000m, response.Land!.SalePrice);
        Assert.Equal(15000m, response.Land.EstimatedCost);
        Assert.Equal(30000m, response.Land.RetailSalePrice);
        Assert.Null(response.Land.Responsibility);
        Assert.Equal(string.Empty, response.Land.LandPurchaseType);
        Assert.Null(response.Land.CustomerLandType);
    }

    // ─── Tax section ──────────────────────────────────────────────────

    [Fact]
    public void Maps_tax_section_with_null_details()
    {
        var package = Package.Create(saleId: 1, name: "P", isPrimary: true);
        package.AddLine(TaxLine.Create(package.Id, 1200m, 0m, 0m, false, details: null));

        var response = PackageDetailMapper.MapToResponse(package);

        Assert.NotNull(response.Tax);
        Assert.Equal(1200m, response.Tax!.SalePrice);
        Assert.False(response.Tax.PreviouslyTitled);
        Assert.Null(response.Tax.TaxExemptionId);
        Assert.Empty(response.Tax.StateTaxQuestionAnswers);
        Assert.Empty(response.Tax.TaxItems);
        Assert.Null(response.Tax.Errors);
    }

    [Fact]
    public void Maps_tax_section_with_default_details()
    {
        var package = Package.Create(saleId: 1, name: "P", isPrimary: true);
        package.AddLine(TaxLine.Create(package.Id, 1200m, 0m, 0m, false, details: TaxDetails.Create(null, null, [], [], null)));

        var response = PackageDetailMapper.MapToResponse(package);

        Assert.NotNull(response.Tax);
        Assert.Empty(response.Tax!.StateTaxQuestionAnswers);
        Assert.Empty(response.Tax.TaxItems);
    }

    // ─── Insurance section ────────────────────────────────────────────

    [Fact]
    public void Insurance_is_null_when_no_line()
    {
        var package = Package.Create(saleId: 1, name: "P", isPrimary: true);

        var response = PackageDetailMapper.MapToResponse(package);

        Assert.Null(response.Insurance);
    }

    [Fact]
    public void Maps_insurance_with_null_details_uses_defaults()
    {
        var package = Package.Create(saleId: 1, name: "P", isPrimary: true);
        package.AddLine(InsuranceLine.Create(
            package.Id, 1500m, 1000m, 2000m, null, false, details: null));

        var response = PackageDetailMapper.MapToResponse(package);

        Assert.NotNull(response.Insurance);
        var ins = response.Insurance!;
        Assert.Equal(1500m, ins.SalePrice);
        Assert.Equal(1000m, ins.EstimatedCost);
        Assert.Equal(2000m, ins.RetailSalePrice);
        Assert.Null(ins.Responsibility);
        Assert.False(ins.ShouldExcludeFromPricing);
        Assert.Equal(string.Empty, ins.InsuranceType);
        Assert.Equal(0m, ins.CoverageAmount);
        Assert.False(ins.HasFoundationOrMasonry);
        Assert.False(ins.InParkOrSubdivision);
        Assert.False(ins.IsLandOwnedByCustomer);
        Assert.False(ins.IsPremiumFinanced);
        Assert.Null(ins.QuoteId);
        Assert.Null(ins.CompanyName);
    }

    // ─── Warranty section ─────────────────────────────────────────────

    [Fact]
    public void Maps_warranty_section_with_null_details()
    {
        var package = Package.Create(saleId: 1, name: "P", isPrimary: true);
        package.AddLine(WarrantyLine.Create(package.Id, 800m, 400m, 900m, false, details: null));

        var response = PackageDetailMapper.MapToResponse(package);

        Assert.NotNull(response.Warranty);
        Assert.Equal(800m, response.Warranty!.SalePrice);
        Assert.False(response.Warranty.WarrantySelected);
        Assert.Null(response.Warranty.WarrantyAmount);
        Assert.Null(response.Warranty.SalesTaxPremium);
        Assert.Null(response.Warranty.QuotedAt);
    }

    // ─── Down payment / Concessions ───────────────────────────────────

    [Fact]
    public void Maps_down_payment_fields()
    {
        var package = Package.Create(saleId: 1, name: "P", isPrimary: true);
        package.AddLine(CreditLine.CreateDownPayment(package.Id, 5000m));

        var response = PackageDetailMapper.MapToResponse(package);

        Assert.NotNull(response.DownPayment);
        Assert.Equal(5000m, response.DownPayment!.SalePrice);
        Assert.Equal(0m, response.DownPayment.EstimatedCost);
        Assert.Equal(0m, response.DownPayment.RetailSalePrice);
        Assert.Equal("Buyer", response.DownPayment.Responsibility);
        Assert.True(response.DownPayment.ShouldExcludeFromPricing);
    }

    [Fact]
    public void Maps_concession_fields()
    {
        var package = Package.Create(saleId: 1, name: "P", isPrimary: true);
        package.AddLine(CreditLine.CreateConcession(package.Id, 2000m));

        var response = PackageDetailMapper.MapToResponse(package);

        Assert.NotNull(response.Concessions);
        Assert.Equal(2000m, response.Concessions!.SalePrice);
        Assert.Equal(0m, response.Concessions.EstimatedCost);
        Assert.Equal(0m, response.Concessions.RetailSalePrice);
        Assert.Equal("Seller", response.Concessions.Responsibility);
        Assert.True(response.Concessions.ShouldExcludeFromPricing);
    }

    [Fact]
    public void Down_payment_and_concession_coexist_independently()
    {
        var package = Package.Create(saleId: 1, name: "P", isPrimary: true);
        package.AddLine(CreditLine.CreateDownPayment(package.Id, 5000m));
        package.AddLine(CreditLine.CreateConcession(package.Id, 2000m));

        var response = PackageDetailMapper.MapToResponse(package);

        Assert.NotNull(response.DownPayment);
        Assert.Equal(5000m, response.DownPayment!.SalePrice);
        Assert.NotNull(response.Concessions);
        Assert.Equal(2000m, response.Concessions!.SalePrice);
    }

    // ─── Trade-ins ────────────────────────────────────────────────────

    [Fact]
    public void Maps_trade_in_lines_ordered_by_sort_order()
    {
        var package = Package.Create(saleId: 1, name: "P", isPrimary: true);
        package.AddLine(TradeInLine.Create(
            package.Id, 8000m, 6000m, 10000m, null, details: null, sortOrder: 2));
        package.AddLine(TradeInLine.Create(
            package.Id, 5000m, 3000m, 7000m, null, details: null, sortOrder: 1));

        var response = PackageDetailMapper.MapToResponse(package);

        Assert.Equal(2, response.TradeIns.Length);
        Assert.Equal(5000m, response.TradeIns[0].SalePrice); // sortOrder 1 first
        Assert.Equal(8000m, response.TradeIns[1].SalePrice); // sortOrder 2 second
    }

    [Fact]
    public void Maps_trade_in_with_null_details_uses_defaults()
    {
        var package = Package.Create(saleId: 1, name: "P", isPrimary: true);
        package.AddLine(TradeInLine.Create(
            package.Id, 5000m, 3000m, 7000m, null, details: null));

        var response = PackageDetailMapper.MapToResponse(package);

        var ti = Assert.Single(response.TradeIns);
        Assert.Equal(5000m, ti.SalePrice);
        Assert.Equal(string.Empty, ti.TradeType);
        Assert.Equal(0, ti.Year);
        Assert.Equal(string.Empty, ti.Make);
        Assert.Equal(string.Empty, ti.Model);
        Assert.Null(ti.FloorWidth);
        Assert.Equal(0m, ti.TradeAllowance);
        Assert.Equal(0m, ti.PayoffAmount);
        Assert.Equal(0m, ti.BookInAmount);
    }

    // ─── Sales team ───────────────────────────────────────────────────

    [Fact]
    public void SalesTeam_is_empty_when_no_line()
    {
        var package = Package.Create(saleId: 1, name: "P", isPrimary: true);

        var response = PackageDetailMapper.MapToResponse(package);

        Assert.Empty(response.SalesTeam);
    }

    [Fact]
    public void SalesTeam_is_empty_when_details_is_null()
    {
        var package = Package.Create(saleId: 1, name: "P", isPrimary: true);
        package.AddLine(SalesTeamLine.Create(package.Id, details: null));

        var response = PackageDetailMapper.MapToResponse(package);

        Assert.Empty(response.SalesTeam);
    }

    [Fact]
    public void SalesTeam_is_empty_when_members_list_is_empty()
    {
        var package = Package.Create(saleId: 1, name: "P", isPrimary: true);
        package.AddLine(SalesTeamLine.Create(package.Id, details: SalesTeamDetails.Create([])));

        var response = PackageDetailMapper.MapToResponse(package);

        Assert.Empty(response.SalesTeam);
    }

    [Fact]
    public void Maps_sales_team_members()
    {
        var package = Package.Create(saleId: 1, name: "P", isPrimary: true);
        var details = SalesTeamDetails.Create([SalesTeamMember.Create(null, SalesTeamRole.Primary, null)]);
        package.AddLine(SalesTeamLine.Create(package.Id, details));

        var response = PackageDetailMapper.MapToResponse(package);

        var member = Assert.Single(response.SalesTeam);
        Assert.Equal("Primary Salesperson", member.Role);
        Assert.Null(member.AuthorizedUserId);
        Assert.Null(member.EmployeeNumber);
        Assert.Equal(0, member.SortOrder);
        Assert.Null(member.CommissionSplitPercentage);
        Assert.Equal(0m, member.CommissionAmount);
    }

    // ─── Project costs ────────────────────────────────────────────────

    [Fact]
    public void Maps_project_cost_with_details()
    {
        var package = Package.Create(saleId: 1, name: "P", isPrimary: true);
        var details = ProjectCostDetails.Create(categoryId: 3, itemId: 7, itemDescription: "Setup Fee");
        package.AddLine(ProjectCostLine.Create(
            package.Id, 1500m, 1000m, 2000m, Responsibility.Buyer, false, details));

        var response = PackageDetailMapper.MapToResponse(package);

        var pc = Assert.Single(response.ProjectCosts);
        Assert.Equal(1500m, pc.SalePrice);
        Assert.Equal(1000m, pc.EstimatedCost);
        Assert.Equal(2000m, pc.RetailSalePrice);
        Assert.Equal("Buyer", pc.Responsibility);
        Assert.False(pc.ShouldExcludeFromPricing);
        Assert.Equal(3, pc.CategoryNumber);
        Assert.Equal(7, pc.ItemId);
        Assert.Equal("Setup Fee", pc.ItemDescription);
    }

    [Fact]
    public void Maps_project_costs_ordered_by_sort_order()
    {
        var package = Package.Create(saleId: 1, name: "P", isPrimary: true);
        package.AddLine(ProjectCostLine.Create(
            package.Id, 2000m, 1500m, 2500m, null, false,
            ProjectCostDetails.Create(1, 2, "B"), sortOrder: 2));
        package.AddLine(ProjectCostLine.Create(
            package.Id, 1000m, 500m, 1200m, null, false,
            ProjectCostDetails.Create(1, 1, "A"), sortOrder: 1));

        var response = PackageDetailMapper.MapToResponse(package);

        Assert.Equal(2, response.ProjectCosts.Length);
        Assert.Equal("A", response.ProjectCosts[0].ItemDescription);
        Assert.Equal("B", response.ProjectCosts[1].ItemDescription);
    }

    [Fact]
    public void Maps_project_cost_with_null_details_uses_defaults()
    {
        var package = Package.Create(saleId: 1, name: "P", isPrimary: true);
        package.AddLine(ProjectCostLine.Create(
            package.Id, 1500m, 1000m, 2000m, null, false, details: null));

        var response = PackageDetailMapper.MapToResponse(package);

        var pc = Assert.Single(response.ProjectCosts);
        Assert.Equal(0, pc.CategoryNumber);
        Assert.Equal(0, pc.ItemId);
        Assert.Null(pc.ItemDescription);
    }

    // ─── Full package ─────────────────────────────────────────────────

    [Fact]
    public void Maps_full_package_with_all_line_types()
    {
        var package = Package.Create(saleId: 1, name: "Full", isPrimary: true);

        var homeDetails = HomeDetails.Create(HomeType.New, HomeSourceType.OnLot, stockNumber: "STK-100");
        package.AddLine(HomeLine.Create(package.Id, 80000m, 50000m, 85000m, Responsibility.Buyer, homeDetails));
        package.AddLine(LandLine.Create(package.Id, 25000m, 15000m, 30000m, null, details: null));
        package.AddLine(TaxLine.Create(package.Id, 1200m, 0m, 0m, false, details: TaxDetails.Create(null, null, [], [], null)));
        package.AddLine(InsuranceLine.Create(package.Id, 1500m, 1000m, 2000m, null, false, details: null));
        package.AddLine(WarrantyLine.Create(package.Id, 800m, 400m, 900m, false, details: null));
        package.AddLine(CreditLine.CreateDownPayment(package.Id, 5000m));
        package.AddLine(CreditLine.CreateConcession(package.Id, 2000m));
        package.AddLine(TradeInLine.Create(package.Id, 8000m, 6000m, 10000m, null, details: null));
        package.AddLine(SalesTeamLine.Create(package.Id, details: SalesTeamDetails.Create([])));
        package.AddLine(ProjectCostLine.Create(
            package.Id, 1500m, 1000m, 2000m, null, false, ProjectCostDetails.Create(1, 1)));

        var response = PackageDetailMapper.MapToResponse(package);

        Assert.NotNull(response.Home);
        Assert.NotNull(response.Land);
        Assert.NotNull(response.Tax);
        Assert.NotNull(response.Insurance);
        Assert.NotNull(response.Warranty);
        Assert.NotNull(response.DownPayment);
        Assert.NotNull(response.Concessions);
        Assert.Single(response.TradeIns);
        Assert.Empty(response.SalesTeam); // SalesTeamDetails has no members
        Assert.Single(response.ProjectCosts);
    }
}
