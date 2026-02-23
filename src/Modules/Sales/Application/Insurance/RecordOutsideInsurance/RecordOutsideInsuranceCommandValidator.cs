using FluentValidation;

namespace Modules.Sales.Application.Insurance.RecordOutsideInsurance;

internal sealed class RecordOutsideInsuranceCommandValidator
    : AbstractValidator<RecordOutsideInsuranceCommand>
{
    public RecordOutsideInsuranceCommandValidator()
    {
        RuleFor(x => x.SalePublicId)
            .NotEmpty();

        RuleFor(x => x.ProviderName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.CoverageAmount)
            .GreaterThan(0);

        RuleFor(x => x.PremiumAmount)
            .GreaterThan(0);
    }
}
