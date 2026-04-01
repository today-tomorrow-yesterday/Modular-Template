using FluentValidation.TestHelper;
using Modules.SampleSales.Application.Products.CreateProduct;
using Xunit;

namespace Modules.SampleSales.Application.Tests.Products;

public sealed class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _sut = new();

    // ─── Name ─────────────────────────────────────────────────────

    [Fact]
    public void Name_empty_should_have_error()
    {
        // Arrange
        var command = new CreateProductCommand("", null, 10.00m, null);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name is required");
    }

    [Fact]
    public void Name_exceeds_max_length_should_have_error()
    {
        // Arrange
        var longName = new string('A', 201);
        var command = new CreateProductCommand(longName, null, 10.00m, null);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name cannot exceed 200 characters");
    }

    [Fact]
    public void Name_valid_should_not_have_error()
    {
        // Arrange
        var command = new CreateProductCommand("Widget", null, 10.00m, null);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    // ─── Description ──────────────────────────────────────────────

    [Fact]
    public void Description_exceeds_max_length_should_have_error()
    {
        // Arrange
        var longDesc = new string('A', 1001);
        var command = new CreateProductCommand("Widget", longDesc, 10.00m, null);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description cannot exceed 1000 characters");
    }

    [Fact]
    public void Description_null_should_not_have_error()
    {
        // Arrange
        var command = new CreateProductCommand("Widget", null, 10.00m, null);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    // ─── Price ────────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public void Price_not_positive_should_have_error(decimal price)
    {
        // Arrange
        var command = new CreateProductCommand("Widget", null, price, null);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Price)
            .WithErrorMessage("Price must be greater than zero");
    }

    [Fact]
    public void Price_positive_should_not_have_error()
    {
        // Arrange
        var command = new CreateProductCommand("Widget", null, 29.99m, null);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Price);
    }

    // ─── All valid ───────────────────────────────────────────────

    [Fact]
    public void Valid_command_should_have_no_errors()
    {
        // Arrange
        var command = new CreateProductCommand("Widget", "A test widget", 29.99m, 10.00m);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
