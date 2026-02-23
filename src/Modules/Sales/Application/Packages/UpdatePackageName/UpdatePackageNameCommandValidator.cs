using FluentValidation;

namespace Modules.Sales.Application.Packages.UpdatePackageName;

internal sealed class UpdatePackageNameCommandValidator
    : AbstractValidator<UpdatePackageNameCommand>
{
    public UpdatePackageNameCommandValidator()
    {
        RuleFor(x => x.PackagePublicId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}
