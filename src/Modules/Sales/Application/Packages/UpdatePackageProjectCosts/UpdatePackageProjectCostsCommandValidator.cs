using FluentValidation;

namespace Modules.Sales.Application.Packages.UpdatePackageProjectCosts;

internal sealed class UpdatePackageProjectCostsCommandValidator
    : AbstractValidator<UpdatePackageProjectCostsCommand>
{
    private static readonly string[] ValidResponsibilities = ["Buyer", "Seller"];

    public UpdatePackageProjectCostsCommandValidator()
    {
        RuleFor(x => x.PackagePublicId)
            .NotEmpty();

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.CategoryId)
                .GreaterThan(0)
                .WithMessage("CategoryId must be a valid project cost category identifier.");

            item.RuleFor(x => x.ItemId)
                .GreaterThan(0)
                .WithMessage("ItemId must be a valid project cost item identifier.");

            item.RuleFor(x => x.SalePrice)
                .GreaterThanOrEqualTo(0);

            item.RuleFor(x => x.EstimatedCost)
                .GreaterThanOrEqualTo(0);

            item.RuleFor(x => x.RetailSalePrice)
                .GreaterThanOrEqualTo(x => x.SalePrice)
                .WithMessage("RetailSalePrice must be greater than or equal to SalePrice.");

            item.RuleFor(x => x.Responsibility)
                .Must(r => ValidResponsibilities.Contains(r, StringComparer.OrdinalIgnoreCase))
                .When(x => !string.IsNullOrEmpty(x.Responsibility))
                .WithMessage("Responsibility must be 'Buyer' or 'Seller'.");
        });
    }
}
