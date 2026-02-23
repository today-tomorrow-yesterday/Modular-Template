using FluentValidation;

namespace Modules.Sales.Application.Insurance.GenerateWarrantyQuote;

internal sealed class GenerateWarrantyQuoteCommandValidator
    : AbstractValidator<GenerateWarrantyQuoteCommand>
{
    public GenerateWarrantyQuoteCommandValidator()
    {
        RuleFor(x => x.SalePublicId)
            .NotEmpty();
    }
}
