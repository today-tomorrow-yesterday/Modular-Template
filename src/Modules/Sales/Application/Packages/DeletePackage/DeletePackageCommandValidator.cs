using FluentValidation;

namespace Modules.Sales.Application.Packages.DeletePackage;

internal sealed class DeletePackageCommandValidator
    : AbstractValidator<DeletePackageCommand>
{
    public DeletePackageCommandValidator()
    {
        RuleFor(x => x.PackagePublicId)
            .NotEmpty();
    }
}
