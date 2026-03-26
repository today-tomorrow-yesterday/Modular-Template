using FluentValidation.TestHelper;
using Modules.Sales.Application.DeliveryAddresses.CreateDeliveryAddress;
using Xunit;

namespace Modules.Sales.Application.Tests.DeliveryAddress;

public sealed class CreateDeliveryAddressCommandValidatorTests
{
    private readonly CreateDeliveryAddressCommandValidator _sut = new();

    [Fact]
    public void Valid_command_passes_validation()
    {
        var command = new CreateDeliveryAddressCommand(
            Guid.NewGuid(),
            "Primary Residence",
            true,
            "123 Main St",
            null,
            "Columbus",
            "Franklin",
            "OH",
            "43004");

        var result = _sut.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_sale_public_id_fails_validation()
    {
        var command = new CreateDeliveryAddressCommand(
            Guid.Empty,
            "Primary Residence",
            true,
            "123 Main St",
            null,
            "Columbus",
            "Franklin",
            "OH",
            "43004");

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.SalePublicId);
    }

    [Fact]
    public void State_longer_than_2_chars_fails_validation()
    {
        var command = new CreateDeliveryAddressCommand(
            Guid.NewGuid(),
            "Primary Residence",
            true,
            "123 Main St",
            null,
            "Columbus",
            "Franklin",
            "Ohio",
            "43004");

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.State);
    }

    [Fact]
    public void PostalCode_longer_than_10_chars_fails_validation()
    {
        var command = new CreateDeliveryAddressCommand(
            Guid.NewGuid(),
            "Primary Residence",
            true,
            "123 Main St",
            null,
            "Columbus",
            "Franklin",
            "OH",
            "43004-12345");

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PostalCode);
    }

    [Fact]
    public void Invalid_occupancy_type_fails_validation()
    {
        var command = new CreateDeliveryAddressCommand(
            Guid.NewGuid(),
            "Commercial",
            true,
            "123 Main St",
            null,
            "Columbus",
            "Franklin",
            "OH",
            "43004");

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.OccupancyType);
    }

    [Fact]
    public void Null_occupancy_type_passes_validation()
    {
        var command = new CreateDeliveryAddressCommand(
            Guid.NewGuid(),
            null,
            true,
            "123 Main St",
            null,
            "Columbus",
            "Franklin",
            "OH",
            "43004");

        var result = _sut.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Null_state_passes_validation()
    {
        var command = new CreateDeliveryAddressCommand(
            Guid.NewGuid(),
            "Primary Residence",
            true,
            "123 Main St",
            null,
            "Columbus",
            "Franklin",
            null,
            "43004");

        var result = _sut.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
