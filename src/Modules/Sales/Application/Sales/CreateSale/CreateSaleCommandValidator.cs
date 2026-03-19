using FluentValidation;

namespace Modules.Sales.Application.Sales.CreateSale;

internal sealed class CreateSaleCommandValidator : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleCommandValidator()
    {
        RuleFor(x => x.CustomerPublicId)
            .NotEmpty();

        RuleFor(x => x.HomeCenterNumber)
            .GreaterThan(0);

        RuleFor(x => x.SaleType)
            .IsInEnum()
            .Must(t => t != Domain.Sales.SaleType.Unknown)
            .WithMessage("SaleType must be a specific type, not Unknown.");
    }
}
