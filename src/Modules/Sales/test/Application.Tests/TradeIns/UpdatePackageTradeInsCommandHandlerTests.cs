using Modules.Sales.Application.Packages.UpdatePackageTradeIns;
using Modules.Sales.Domain;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Details;
using Modules.Sales.Domain.Packages.Lines;
using NSubstitute;
using Rtl.Core.Application.Persistence;
using Xunit;

namespace Modules.Sales.Application.Tests.TradeIns;

public sealed class UpdatePackageTradeInsCommandHandlerTests
{
    private readonly IPackageRepository _packageRepository = Substitute.For<IPackageRepository>();
    private readonly IUnitOfWork<ISalesModule> _unitOfWork = Substitute.For<IUnitOfWork<ISalesModule>>();
    private readonly UpdatePackageTradeInsCommandHandler _sut;

    public UpdatePackageTradeInsCommandHandlerTests() => _sut = new UpdatePackageTradeInsCommandHandler(_packageRepository, _unitOfWork);

    private static UpdatePackageTradeInItemRequest ValidItem(
        decimal salePrice = 15000m,
        decimal tradeAllowance = 25000m,
        decimal bookInAmount = 18000m) => new(
        SalePrice: salePrice,
        EstimatedCost: 0m,
        RetailSalePrice: salePrice,
        TradeType: "Single Wide",
        Year: 2020,
        Make: "Clayton",
        Model: "TruMH",
        FloorWidth: null,
        FloorLength: null,
        TradeAllowance: tradeAllowance,
        PayoffAmount: 10000m,
        BookInAmount: bookInAmount);

    [Fact]
    public async Task Returns_failure_when_package_not_found()
    {
        var publicId = Guid.NewGuid();
        _packageRepository.GetByPublicIdWithSaleContextAsync(publicId, Arg.Any<CancellationToken>())
            .Returns((Package?)null);

        var result = await _sut.Handle(
            new UpdatePackageTradeInsCommand(publicId, [ValidItem()]), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Package.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Creates_trade_in_lines_when_none_exist()
    {
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        var result = await _sut.Handle(
            new UpdatePackageTradeInsCommand(package.PublicId, [ValidItem(10000m), ValidItem(8000m)]),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var tradeIns = package.Lines.OfType<TradeInLine>().ToList();
        Assert.Equal(2, tradeIns.Count);
        Assert.Equal(10000m, tradeIns[0].SalePrice);
        Assert.Equal(8000m, tradeIns[1].SalePrice);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Replaces_existing_trade_in_lines_with_new_ones()
    {
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        var details = TradeInDetails.Create("Old Type", 2018, "OldMake", "OldModel", 5000m, 0m, 0m);
        package.AddLine(TradeInLine.Create(package.Id, 5000m, 0m, 0m, Responsibility.Buyer, details, 0));

        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        var result = await _sut.Handle(
            new UpdatePackageTradeInsCommand(package.PublicId, [ValidItem(20000m)]),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var tradeIns = package.Lines.OfType<TradeInLine>().ToList();
        Assert.Single(tradeIns);
        Assert.Equal(20000m, tradeIns[0].SalePrice);
    }

    [Fact]
    public async Task Empty_items_array_removes_all_trade_ins()
    {
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        var details = TradeInDetails.Create("Type", 2020, "Make", "Model", 10000m, 0m, 0m);
        package.AddLine(TradeInLine.Create(package.Id, 10000m, 0m, 0m, Responsibility.Buyer, details, 0));
        package.AddLine(TradeInLine.Create(package.Id, 8000m, 0m, 0m, Responsibility.Buyer, details, 1));

        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        var result = await _sut.Handle(
            new UpdatePackageTradeInsCommand(package.PublicId, []),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(package.Lines.OfType<TradeInLine>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Trade_in_does_not_affect_gross_profit()
    {
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);
        var gpBefore = package.GrossProfit;

        var result = await _sut.Handle(
            new UpdatePackageTradeInsCommand(package.PublicId, [ValidItem(50000m)]),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        // Trade-in lines are excluded from pricing. However, Trade Over Allowance PCs reduce GP.
        // ValidItem has TradeAllowance=25000 > BookInAmount=18000, so a Trade Over Allowance PC is created.
        // GP = gpBefore - 7000 (Trade Over Allowance EstimatedCost)
        Assert.Equal(gpBefore - 7000m, result.Value.GrossProfit);
    }

    [Fact]
    public async Task Result_contains_correct_gross_profit_values()
    {
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        var result = await _sut.Handle(
            new UpdatePackageTradeInsCommand(package.PublicId, [ValidItem()]),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(package.GrossProfit, result.Value.GrossProfit);
        Assert.Equal(package.CommissionableGrossProfit, result.Value.CommissionableGrossProfit);
        Assert.Equal(package.MustRecalculateTaxes, result.Value.MustRecalculateTaxes);
    }

    [Fact]
    public async Task Trade_in_lines_use_request_estimated_cost_and_retail_sale_price()
    {
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        var item = ValidItem() with { EstimatedCost = 500m, RetailSalePrice = 16000m };

        await _sut.Handle(
            new UpdatePackageTradeInsCommand(package.PublicId, [item]),
            CancellationToken.None);

        var tradeIn = Assert.Single(package.Lines.OfType<TradeInLine>());
        Assert.Equal(500m, tradeIn.EstimatedCost);
        Assert.Equal(16000m, tradeIn.RetailSalePrice);
    }

    [Fact]
    public async Task Trade_in_lines_have_buyer_responsibility()
    {
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        await _sut.Handle(
            new UpdatePackageTradeInsCommand(package.PublicId, [ValidItem()]),
            CancellationToken.None);

        var tradeIn = Assert.Single(package.Lines.OfType<TradeInLine>());
        Assert.Equal(Responsibility.Buyer, tradeIn.Responsibility);
    }

    [Fact]
    public async Task Trade_in_lines_have_sequential_sort_order()
    {
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        await _sut.Handle(
            new UpdatePackageTradeInsCommand(package.PublicId, [ValidItem(10000m), ValidItem(8000m), ValidItem(5000m)]),
            CancellationToken.None);

        var tradeIns = package.Lines.OfType<TradeInLine>().ToList();
        Assert.Equal(0, tradeIns[0].SortOrder);
        Assert.Equal(1, tradeIns[1].SortOrder);
        Assert.Equal(2, tradeIns[2].SortOrder);
    }

    [Fact]
    public async Task Trade_in_details_are_correctly_mapped()
    {
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        var item = new UpdatePackageTradeInItemRequest(
            SalePrice: 15000m,
            EstimatedCost: 0m,
            RetailSalePrice: 15000m,
            TradeType: "Double Wide",
            Year: 2019,
            Make: "Clayton",
            Model: "iHouse",
            FloorWidth: 28m,
            FloorLength: 56m,
            TradeAllowance: 30000m,
            PayoffAmount: 12000m,
            BookInAmount: 22000m);

        await _sut.Handle(
            new UpdatePackageTradeInsCommand(package.PublicId, [item]),
            CancellationToken.None);

        var tradeIn = Assert.Single(package.Lines.OfType<TradeInLine>());
        Assert.NotNull(tradeIn.Details);
        Assert.Equal("Double Wide", tradeIn.Details.TradeType);
        Assert.Equal(2019, tradeIn.Details.Year);
        Assert.Equal("Clayton", tradeIn.Details.Make);
        Assert.Equal("iHouse", tradeIn.Details.Model);
        Assert.Equal(28m, tradeIn.Details.FloorWidth);
        Assert.Equal(56m, tradeIn.Details.FloorLength);
        Assert.Equal(30000m, tradeIn.Details.TradeAllowance);
        Assert.Equal(12000m, tradeIn.Details.PayoffAmount);
        Assert.Equal(22000m, tradeIn.Details.BookInAmount);
    }

    [Fact]
    public async Task Does_not_remove_other_line_types()
    {
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        package.AddLine(CreditLine.CreateDownPayment(package.Id, 5000m));
        var details = TradeInDetails.Create("Type", 2020, "Make", "Model", 10000m, 0m, 0m);
        package.AddLine(TradeInLine.Create(package.Id, 10000m, 0m, 0m, Responsibility.Buyer, details, 0));

        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        await _sut.Handle(
            new UpdatePackageTradeInsCommand(package.PublicId, [ValidItem()]),
            CancellationToken.None);

        Assert.Single(package.Lines.OfType<CreditLine>());
        Assert.Single(package.Lines.OfType<TradeInLine>());
    }

    [Fact]
    public async Task Sale_price_is_rounded_to_two_decimal_places()
    {
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        var item = ValidItem() with { SalePrice = 1234.5678m };

        await _sut.Handle(
            new UpdatePackageTradeInsCommand(package.PublicId, [item]),
            CancellationToken.None);

        var tradeIn = Assert.Single(package.Lines.OfType<TradeInLine>());
        Assert.Equal(1234.57m, tradeIn.SalePrice);
    }

    // --- Trade Over Allowance project cost sync tests ---

    [Fact]
    public async Task Creates_trade_over_allowance_pc_when_trade_allowance_exceeds_book_in()
    {
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        // TradeAllowance(25000) - BookInAmount(18000) = 7000 > 0 → create PC
        await _sut.Handle(
            new UpdatePackageTradeInsCommand(package.PublicId, [ValidItem()]),
            CancellationToken.None);

        var pcs = package.Lines.OfType<ProjectCostLine>()
            .Where(l => l.Details?.CategoryId == 10 && l.Details?.ItemId == 9)
            .ToList();
        Assert.Single(pcs);
        Assert.Equal(7000m, pcs[0].EstimatedCost);
        Assert.Equal(0m, pcs[0].SalePrice);
        Assert.Equal(Responsibility.Seller, pcs[0].Responsibility);
        Assert.False(pcs[0].ShouldExcludeFromPricing);
    }

    [Fact]
    public async Task Does_not_create_trade_over_allowance_when_book_in_exceeds_allowance()
    {
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        // TradeAllowance(10000) - BookInAmount(18000) = -8000 ≤ 0 → no PC
        var item = ValidItem(tradeAllowance: 10000m, bookInAmount: 18000m);

        await _sut.Handle(
            new UpdatePackageTradeInsCommand(package.PublicId, [item]),
            CancellationToken.None);

        var pcs = package.Lines.OfType<ProjectCostLine>()
            .Where(l => l.Details?.CategoryId == 10 && l.Details?.ItemId == 9)
            .ToList();
        Assert.Empty(pcs);
    }

    [Fact]
    public async Task Creates_multiple_trade_over_allowance_pcs_for_multiple_qualifying_trade_ins()
    {
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        var item1 = ValidItem(tradeAllowance: 20000m, bookInAmount: 15000m); // 5000 over
        var item2 = ValidItem(tradeAllowance: 30000m, bookInAmount: 22000m); // 8000 over
        var item3 = ValidItem(tradeAllowance: 5000m, bookInAmount: 10000m);  // no over

        await _sut.Handle(
            new UpdatePackageTradeInsCommand(package.PublicId, [item1, item2, item3]),
            CancellationToken.None);

        var pcs = package.Lines.OfType<ProjectCostLine>()
            .Where(l => l.Details?.CategoryId == 10 && l.Details?.ItemId == 9)
            .OrderBy(l => l.EstimatedCost)
            .ToList();
        Assert.Equal(2, pcs.Count);
        Assert.Equal(5000m, pcs[0].EstimatedCost);
        Assert.Equal(8000m, pcs[1].EstimatedCost);
    }

    [Fact]
    public async Task Removes_existing_trade_over_allowance_pcs_when_trade_ins_cleared()
    {
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        // Simulate existing trade-in + Trade Over Allowance PC
        var tradeDetails = TradeInDetails.Create("Type", 2020, "Make", "Model", 20000m, 0m, 15000m);
        package.AddLine(TradeInLine.Create(package.Id, 10000m, 0m, 0m, Responsibility.Buyer, tradeDetails, 0));
        var pcDetails = ProjectCostDetails.Create(categoryId: 10, itemId: 9, itemDescription: "Trade Over Allowance");
        package.AddLine(ProjectCostLine.Create(package.Id, 0m, 5000m, 0m, Responsibility.Seller, false, pcDetails));

        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        // Empty items → remove all trade-ins AND their Trade Over Allowance PCs
        await _sut.Handle(
            new UpdatePackageTradeInsCommand(package.PublicId, []),
            CancellationToken.None);

        Assert.Empty(package.Lines.OfType<TradeInLine>());
        var pcs = package.Lines.OfType<ProjectCostLine>()
            .Where(l => l.Details?.CategoryId == 10 && l.Details?.ItemId == 9);
        Assert.Empty(pcs);
    }

    [Fact]
    public async Task Trade_over_allowance_reduces_gross_profit()
    {
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);
        var gpBefore = package.GrossProfit;

        // TradeAllowance(30000) - BookInAmount(20000) = 10000 Trade Over Allowance
        var item = ValidItem(tradeAllowance: 30000m, bookInAmount: 20000m);

        await _sut.Handle(
            new UpdatePackageTradeInsCommand(package.PublicId, [item]),
            CancellationToken.None);

        // Trade Over Allowance PC has EstimatedCost=10000, SalePrice=0, ShouldExcludeFromPricing=false
        // GP = SUM(SalePrice - EstimatedCost) WHERE NOT excluded = 0 - 10000 = -10000
        Assert.Equal(gpBefore - 10000m, package.GrossProfit);
    }

    // --- Tax change detection tests ---

    [Fact]
    public async Task Flags_tax_recalculation_when_trade_in_prices_change()
    {
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        var details = TradeInDetails.Create("Type", 2020, "Make", "Model", 10000m, 0m, 10000m);
        package.AddLine(TradeInLine.Create(package.Id, 5000m, 0m, 0m, Responsibility.Buyer, details, 0));

        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        // Change trade-in SalePrice from 5000 to 8000. TradeAllowance=BookInAmount so no PC change.
        var item = ValidItem(salePrice: 8000m, tradeAllowance: 10000m, bookInAmount: 10000m);

        var result = await _sut.Handle(
            new UpdatePackageTradeInsCommand(package.PublicId, [item]),
            CancellationToken.None);

        Assert.True(result.Value.MustRecalculateTaxes);
    }

    [Fact]
    public async Task Flags_tax_recalculation_when_trade_over_allowance_added()
    {
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        // Existing trade-in with no over-allowance (TradeAllowance = BookInAmount)
        var details = TradeInDetails.Create("Type", 2020, "Make", "Model", 10000m, 0m, 10000m);
        package.AddLine(TradeInLine.Create(package.Id, 5000m, 0m, 0m, Responsibility.Buyer, details, 0));

        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        // New trade-in with over-allowance → adds a PC (non-excluded count changes)
        var item = ValidItem(salePrice: 5000m, tradeAllowance: 25000m, bookInAmount: 18000m);

        var result = await _sut.Handle(
            new UpdatePackageTradeInsCommand(package.PublicId, [item]),
            CancellationToken.None);

        Assert.True(result.Value.MustRecalculateTaxes);
    }

    [Fact]
    public async Task Does_not_flag_tax_recalculation_when_nothing_changes()
    {
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        package.ClearTaxRecalculationFlag(); // Package.Create defaults to true
        // Existing trade-in with no over-allowance
        var details = TradeInDetails.Create("Single Wide", 2020, "Clayton", "TruMH", 10000m, 10000m, 10000m);
        package.AddLine(TradeInLine.Create(package.Id, 15000m, 0m, 15000m, Responsibility.Buyer, details, 0));

        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        // Same SalePrice, same no-over-allowance → nothing changes
        var item = ValidItem(salePrice: 15000m, tradeAllowance: 10000m, bookInAmount: 10000m);

        var result = await _sut.Handle(
            new UpdatePackageTradeInsCommand(package.PublicId, [item]),
            CancellationToken.None);

        Assert.False(result.Value.MustRecalculateTaxes);
    }

    [Fact]
    public async Task Removes_use_tax_project_cost_on_tax_change()
    {
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        // Pre-existing Use Tax project cost (Cat 9, Item 21)
        var useTaxDetails = ProjectCostDetails.Create(categoryId: 9, itemId: 21, itemDescription: "Use Tax");
        package.AddLine(ProjectCostLine.Create(package.Id, 500m, 500m, 0m, Responsibility.Seller, false, useTaxDetails));

        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        // Adding a trade-in with over-allowance changes project cost count → triggers tax cascade
        var item = ValidItem(tradeAllowance: 25000m, bookInAmount: 18000m);

        await _sut.Handle(
            new UpdatePackageTradeInsCommand(package.PublicId, [item]),
            CancellationToken.None);

        var useTaxPcs = package.Lines.OfType<ProjectCostLine>()
            .Where(l => l.Details?.CategoryId == 9 && l.Details?.ItemId == 21);
        Assert.Empty(useTaxPcs);
    }

    [Fact]
    public async Task Floor_dimensions_passed_to_details()
    {
        var package = Package.Create(saleId: 1, name: "Primary", isPrimary: true);
        _packageRepository.GetByPublicIdWithSaleContextAsync(package.PublicId, Arg.Any<CancellationToken>())
            .Returns(package);

        var item = ValidItem() with { FloorWidth = 16m, FloorLength = 76m };

        await _sut.Handle(
            new UpdatePackageTradeInsCommand(package.PublicId, [item]),
            CancellationToken.None);

        var tradeIn = Assert.Single(package.Lines.OfType<TradeInLine>());
        Assert.Equal(16m, tradeIn.Details!.FloorWidth);
        Assert.Equal(76m, tradeIn.Details!.FloorLength);
    }
}
