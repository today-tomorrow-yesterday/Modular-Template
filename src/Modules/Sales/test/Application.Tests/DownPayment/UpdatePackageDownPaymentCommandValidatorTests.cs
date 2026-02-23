using FluentValidation.TestHelper;
using Modules.Sales.Application.Packages.UpdatePackageDownPayment;
using Xunit;

namespace Modules.Sales.Application.Tests.DownPayment;

public sealed class UpdatePackageDownPaymentCommandValidatorTests
{
    private readonly UpdatePackageDownPaymentCommandValidator _sut = new();

    [Fact]
    public void Valid_command_passes_validation()
    {
        var command = new UpdatePackageDownPaymentCommand(Guid.NewGuid(), 5000m);

        var result = _sut.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Zero_amount_passes_validation()
    {
        var command = new UpdatePackageDownPaymentCommand(Guid.NewGuid(), 0m);

        var result = _sut.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Max_amount_passes_validation()
    {
        var command = new UpdatePackageDownPaymentCommand(Guid.NewGuid(), 1_000_000m);

        var result = _sut.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_package_public_id_fails_validation()
    {
        var command = new UpdatePackageDownPaymentCommand(Guid.Empty, 5000m);

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PackagePublicId);
    }

    [Fact]
    public void Negative_amount_fails_validation()
    {
        var command = new UpdatePackageDownPaymentCommand(Guid.NewGuid(), -1m);

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Amount_exceeding_max_fails_validation()
    {
        var command = new UpdatePackageDownPaymentCommand(Guid.NewGuid(), 1_000_001m);

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }
}
