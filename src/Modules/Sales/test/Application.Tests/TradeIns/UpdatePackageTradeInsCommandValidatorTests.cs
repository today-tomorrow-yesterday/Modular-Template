using FluentValidation.TestHelper;
using Modules.Sales.Application.Packages.UpdatePackageTradeIns;
using Xunit;

namespace Modules.Sales.Application.Tests.TradeIns;

public sealed class UpdatePackageTradeInsCommandValidatorTests
{
    private readonly UpdatePackageTradeInsCommandValidator _sut = new();

    private static UpdatePackageTradeInItemRequest ValidItem() => new(
        SalePrice: 15000m,
        EstimatedCost: 0m,
        RetailSalePrice: 15000m,
        TradeType: "Auto",
        Year: 2020,
        Make: "Clayton",
        Model: "TruMH",
        FloorWidth: null,
        FloorLength: null,
        TradeAllowance: 25000m,
        PayoffAmount: 10000m,
        BookInAmount: 18000m);

    [Fact]
    public void Valid_command_with_one_item_passes_validation()
    {
        var command = new UpdatePackageTradeInsCommand(Guid.NewGuid(), [ValidItem()]);

        var result = _sut.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_items_array_passes_validation()
    {
        var command = new UpdatePackageTradeInsCommand(Guid.NewGuid(), []);

        var result = _sut.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Five_items_passes_validation()
    {
        var items = Enumerable.Range(0, 5).Select(_ => ValidItem()).ToArray();
        var command = new UpdatePackageTradeInsCommand(Guid.NewGuid(), items);

        var result = _sut.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Six_items_fails_validation()
    {
        var items = Enumerable.Range(0, 6).Select(_ => ValidItem()).ToArray();
        var command = new UpdatePackageTradeInsCommand(Guid.NewGuid(), items);

        var result = _sut.TestValidate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Empty_package_public_id_fails_validation()
    {
        var command = new UpdatePackageTradeInsCommand(Guid.Empty, [ValidItem()]);

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PackagePublicId);
    }

    [Fact]
    public void Negative_sale_price_fails_validation()
    {
        var item = ValidItem() with { SalePrice = -1m };
        var command = new UpdatePackageTradeInsCommand(Guid.NewGuid(), [item]);

        var result = _sut.TestValidate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Zero_sale_price_passes_validation()
    {
        var item = ValidItem() with { SalePrice = 0m };
        var command = new UpdatePackageTradeInsCommand(Guid.NewGuid(), [item]);

        var result = _sut.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_trade_type_fails_validation()
    {
        var item = ValidItem() with { TradeType = "" };
        var command = new UpdatePackageTradeInsCommand(Guid.NewGuid(), [item]);

        var result = _sut.TestValidate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Year_1900_fails_validation()
    {
        var item = ValidItem() with { Year = 1900 };
        var command = new UpdatePackageTradeInsCommand(Guid.NewGuid(), [item]);

        var result = _sut.TestValidate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Year_1901_passes_validation()
    {
        var item = ValidItem() with { Year = 1901 };
        var command = new UpdatePackageTradeInsCommand(Guid.NewGuid(), [item]);

        var result = _sut.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_make_fails_validation()
    {
        var item = ValidItem() with { Make = "" };
        var command = new UpdatePackageTradeInsCommand(Guid.NewGuid(), [item]);

        var result = _sut.TestValidate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Empty_model_fails_validation()
    {
        var item = ValidItem() with { Model = "" };
        var command = new UpdatePackageTradeInsCommand(Guid.NewGuid(), [item]);

        var result = _sut.TestValidate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Negative_trade_allowance_fails_validation()
    {
        var item = ValidItem() with { TradeAllowance = -1m };
        var command = new UpdatePackageTradeInsCommand(Guid.NewGuid(), [item]);

        var result = _sut.TestValidate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Negative_payoff_amount_fails_validation()
    {
        var item = ValidItem() with { PayoffAmount = -1m };
        var command = new UpdatePackageTradeInsCommand(Guid.NewGuid(), [item]);

        var result = _sut.TestValidate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Negative_book_in_amount_fails_validation()
    {
        var item = ValidItem() with { BookInAmount = -1m };
        var command = new UpdatePackageTradeInsCommand(Guid.NewGuid(), [item]);

        var result = _sut.TestValidate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Zero_trade_allowance_passes_validation()
    {
        var item = ValidItem() with { TradeAllowance = 0m };
        var command = new UpdatePackageTradeInsCommand(Guid.NewGuid(), [item]);

        var result = _sut.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Zero_payoff_amount_passes_validation()
    {
        var item = ValidItem() with { PayoffAmount = 0m };
        var command = new UpdatePackageTradeInsCommand(Guid.NewGuid(), [item]);

        var result = _sut.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Zero_book_in_amount_passes_validation()
    {
        var item = ValidItem() with { BookInAmount = 0m };
        var command = new UpdatePackageTradeInsCommand(Guid.NewGuid(), [item]);

        var result = _sut.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Negative_estimated_cost_fails_validation()
    {
        var item = ValidItem() with { EstimatedCost = -1m };
        var command = new UpdatePackageTradeInsCommand(Guid.NewGuid(), [item]);

        var result = _sut.TestValidate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Negative_retail_sale_price_fails_validation()
    {
        var item = ValidItem() with { RetailSalePrice = -1m };
        var command = new UpdatePackageTradeInsCommand(Guid.NewGuid(), [item]);

        var result = _sut.TestValidate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Zero_floor_width_for_non_home_type_passes_validation()
    {
        // "Auto" is not a home trade type, so FloorWidth >= 0 is valid
        var item = ValidItem() with { FloorWidth = 0m };
        var command = new UpdatePackageTradeInsCommand(Guid.NewGuid(), [item]);

        var result = _sut.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Negative_floor_width_fails_validation()
    {
        var item = ValidItem() with { FloorWidth = -1m };
        var command = new UpdatePackageTradeInsCommand(Guid.NewGuid(), [item]);

        var result = _sut.TestValidate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Positive_floor_width_passes_validation()
    {
        var item = ValidItem() with { FloorWidth = 16m };
        var command = new UpdatePackageTradeInsCommand(Guid.NewGuid(), [item]);

        var result = _sut.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Null_floor_width_passes_validation()
    {
        var item = ValidItem() with { FloorWidth = null };
        var command = new UpdatePackageTradeInsCommand(Guid.NewGuid(), [item]);

        var result = _sut.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Zero_floor_length_for_non_home_type_passes_validation()
    {
        // "Auto" is not a home trade type, so FloorLength >= 0 is valid
        var item = ValidItem() with { FloorLength = 0m };
        var command = new UpdatePackageTradeInsCommand(Guid.NewGuid(), [item]);

        var result = _sut.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Positive_floor_length_passes_validation()
    {
        var item = ValidItem() with { FloorLength = 76m };
        var command = new UpdatePackageTradeInsCommand(Guid.NewGuid(), [item]);

        var result = _sut.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
