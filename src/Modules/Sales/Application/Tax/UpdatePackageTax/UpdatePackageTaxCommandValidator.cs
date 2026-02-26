using FluentValidation;

namespace Modules.Sales.Application.Tax.UpdatePackageTax;

internal sealed class UpdatePackageTaxCommandValidator
    : AbstractValidator<UpdatePackageTaxCommand>
{
    private static readonly string[] ValidPreviouslyTitledValues = ["Y", "N"];

    public UpdatePackageTaxCommandValidator()
    {
        RuleFor(x => x.PackagePublicId)
            .NotEmpty();

        // Rule 1 & 2: PreviouslyTitled is required and must be "Y" or "N"
        RuleFor(x => x.PreviouslyTitled)
            .NotEmpty()
            .WithMessage("PreviouslyTitled is required.");

        RuleFor(x => x.PreviouslyTitled)
            .Must(v => ValidPreviouslyTitledValues.Contains(v, StringComparer.OrdinalIgnoreCase))
            .When(x => !string.IsNullOrEmpty(x.PreviouslyTitled))
            .WithMessage($"PreviouslyTitled must be one of: {string.Join(", ", ValidPreviouslyTitledValues)}.");

        // Rule 3: TaxExemptionId must be greater than 0 when provided (non-null means tax-exempt)
        RuleFor(x => x.TaxExemptionId)
            .GreaterThan(0)
            .When(x => x.TaxExemptionId is not null)
            .WithMessage("TaxExemptionId must be greater than zero when provided.");

        // Rule 5: QuestionAnswers collection must not be null (empty is OK)
        RuleFor(x => x.QuestionAnswers)
            .NotNull()
            .WithMessage("QuestionAnswers cannot be null.");

        // Rule 6: Each QuestionAnswer must have a valid QuestionNumber (> 0)
        RuleForEach(x => x.QuestionAnswers)
            .ChildRules(answer =>
            {
                answer.RuleFor(a => a.QuestionNumber)
                    .GreaterThan(0)
                    .WithMessage("QuestionNumber must be greater than zero.");
            })
            .When(x => x.QuestionAnswers is not null);
    }
}
