using FluentValidation;

namespace Modules.Sales.Application.Packages.CreatePackage;

internal sealed class CreatePackageCommandValidator
    : AbstractValidator<CreatePackageCommand>
{
    public CreatePackageCommandValidator()
    {
        RuleFor(x => x.SalePublicId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}
