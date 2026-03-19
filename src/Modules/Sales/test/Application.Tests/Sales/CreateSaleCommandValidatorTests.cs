using FluentValidation.TestHelper;
using Modules.Sales.Application.Sales.CreateSale;
using Modules.Sales.Domain.Sales;
using Xunit;

namespace Modules.Sales.Application.Tests.Sales;

public sealed class CreateSaleCommandValidatorTests
{
    private readonly CreateSaleCommandValidator _sut = new();

    [Fact]
    public void Valid_command_passes_validation()
    {
        var command = new CreateSaleCommand(Guid.NewGuid(), 42);

        var result = _sut.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Valid_command_with_explicit_sale_type_passes_validation()
    {
        var command = new CreateSaleCommand(Guid.NewGuid(), 42, SaleType.B2B);

        var result = _sut.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_customer_public_id_fails_validation()
    {
        var command = new CreateSaleCommand(Guid.Empty, 42);

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CustomerPublicId);
    }

    [Fact]
    public void HomeCenterNumber_zero_fails_validation()
    {
        var command = new CreateSaleCommand(Guid.NewGuid(), 0);

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.HomeCenterNumber);
    }

    [Fact]
    public void HomeCenterNumber_negative_fails_validation()
    {
        var command = new CreateSaleCommand(Guid.NewGuid(), -1);

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.HomeCenterNumber);
    }

    [Fact]
    public void Invalid_sale_type_fails_validation()
    {
        var command = new CreateSaleCommand(Guid.NewGuid(), 42, (SaleType)999);

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.SaleType);
    }
}
