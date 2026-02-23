using FluentValidation;

namespace Modules.SampleSales.Application.Catalogs.CreateCatalog;

internal sealed class CreateCatalogCommandValidator : AbstractValidator<CreateCatalogCommand>
{
    public CreateCatalogCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(200)
            .WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description cannot exceed 1000 characters");
    }
}
