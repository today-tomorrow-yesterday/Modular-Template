using FluentValidation;

namespace Modules.Sales.Application.Packages.UpdatePackageConcessions;

internal sealed class UpdatePackageConcessionsCommandValidator
    : AbstractValidator<UpdatePackageConcessionsCommand>
{
    public UpdatePackageConcessionsCommandValidator()
    {
        RuleFor(x => x.PackagePublicId)
            .NotEmpty();

        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(1_000_000);
    }
}
