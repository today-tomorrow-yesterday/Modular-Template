using FluentValidation;

namespace Modules.Sales.Application.Packages.UpdatePackageDownPayment;

internal sealed class UpdatePackageDownPaymentCommandValidator
    : AbstractValidator<UpdatePackageDownPaymentCommand>
{
    public UpdatePackageDownPaymentCommandValidator()
    {
        RuleFor(x => x.PackagePublicId)
            .NotEmpty();

        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(1_000_000);
    }
}
