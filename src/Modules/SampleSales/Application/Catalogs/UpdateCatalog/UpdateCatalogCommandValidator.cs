using FluentValidation;

namespace Modules.SampleSales.Application.Catalogs.UpdateCatalog;

internal sealed class UpdateCatalogCommandValidator : AbstractValidator<UpdateCatalogCommand>
{
    public UpdateCatalogCommandValidator()
    {
        RuleFor(x => x.PublicCatalogId)
            .NotEmpty()
            .WithMessage("PublicCatalogId is required.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Catalog name is required.")
            .MaximumLength(200)
            .WithMessage("Catalog name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Catalog description must not exceed 1000 characters.");
    }
}
