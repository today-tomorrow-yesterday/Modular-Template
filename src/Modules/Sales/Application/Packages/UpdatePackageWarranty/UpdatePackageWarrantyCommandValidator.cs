using FluentValidation;

namespace Modules.Sales.Application.Packages.UpdatePackageWarranty;

internal sealed class UpdatePackageWarrantyCommandValidator
    : AbstractValidator<UpdatePackageWarrantyCommand>
{
    public UpdatePackageWarrantyCommandValidator()
    {
        RuleFor(x => x.PackagePublicId)
            .NotEmpty();

        RuleFor(x => x.WarrantyAmount)
            .GreaterThanOrEqualTo(0);
    }
}
