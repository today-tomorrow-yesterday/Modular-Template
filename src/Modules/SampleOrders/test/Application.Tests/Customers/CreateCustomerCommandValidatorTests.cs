using FluentValidation.TestHelper;
using Modules.SampleOrders.Application.Customers.CreateCustomer;
using Xunit;

namespace Modules.SampleOrders.Application.Tests.Customers;

public sealed class CreateCustomerCommandValidatorTests
{
    private readonly CreateCustomerCommandValidator _sut = new();

    // ─── FirstName ────────────────────────────────────────────────

    [Fact]
    public void FirstName_empty_should_have_error()
    {
        // Arrange
        var command = new CreateCustomerCommand("", null, "Doe", null);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("First name is required.");
    }

    [Fact]
    public void FirstName_exceeds_max_length_should_have_error()
    {
        // Arrange
        var longName = new string('A', 101);
        var command = new CreateCustomerCommand(longName, null, "Doe", null);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("First name cannot exceed 100 characters.");
    }

    [Fact]
    public void FirstName_valid_should_not_have_error()
    {
        // Arrange
        var command = new CreateCustomerCommand("John", null, "Doe", null);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
    }

    // ─── LastName ─────────────────────────────────────────────────

    [Fact]
    public void LastName_empty_should_have_error()
    {
        // Arrange
        var command = new CreateCustomerCommand("John", null, "", null);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("Last name is required.");
    }

    [Fact]
    public void LastName_exceeds_max_length_should_have_error()
    {
        // Arrange
        var longName = new string('B', 101);
        var command = new CreateCustomerCommand("John", null, longName, null);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("Last name cannot exceed 100 characters.");
    }

    [Fact]
    public void LastName_valid_should_not_have_error()
    {
        // Arrange
        var command = new CreateCustomerCommand("John", null, "Doe", null);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.LastName);
    }

    // ─── MiddleName ──────────────────────────────────────────────

    [Fact]
    public void MiddleName_exceeds_max_length_should_have_error()
    {
        // Arrange
        var longName = new string('C', 101);
        var command = new CreateCustomerCommand("John", longName, "Doe", null);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MiddleName)
            .WithErrorMessage("Middle name cannot exceed 100 characters.");
    }

    [Fact]
    public void MiddleName_null_should_not_have_error()
    {
        // Arrange
        var command = new CreateCustomerCommand("John", null, "Doe", null);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MiddleName);
    }

    // ─── Email ───────────────────────────────────────────────────

    [Fact]
    public void Email_invalid_format_should_have_error()
    {
        // Arrange
        var command = new CreateCustomerCommand("John", null, "Doe", "not-an-email");

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email format is invalid.");
    }

    [Fact]
    public void Email_valid_format_should_not_have_error()
    {
        // Arrange
        var command = new CreateCustomerCommand("John", null, "Doe", "john@example.com");

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Email_null_should_not_have_error()
    {
        // Arrange
        var command = new CreateCustomerCommand("John", null, "Doe", null);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Email_empty_string_should_not_have_email_format_error()
    {
        // Arrange — the .When() condition skips email validation for empty strings
        var command = new CreateCustomerCommand("John", null, "Doe", "");

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    // ─── All valid ───────────────────────────────────────────────

    [Fact]
    public void Valid_command_should_have_no_errors()
    {
        // Arrange
        var command = new CreateCustomerCommand("John", "M", "Doe", "john@example.com", new DateOnly(1990, 1, 1));

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
