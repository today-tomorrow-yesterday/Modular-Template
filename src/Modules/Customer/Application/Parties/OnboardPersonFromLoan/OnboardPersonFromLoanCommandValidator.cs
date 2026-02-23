using FluentValidation;

namespace Modules.Customer.Application.Parties.OnboardPersonFromLoan;

internal sealed class OnboardPersonFromLoanCommandValidator : AbstractValidator<OnboardPersonFromLoanCommand>
{
    public OnboardPersonFromLoanCommandValidator()
    {
        RuleFor(x => x.LoanId)
            .NotEmpty()
            .WithMessage("LoanId is required");

        RuleFor(x => x.HomeCenterNumber)
            .GreaterThan(0)
            .WithMessage("HomeCenterNumber is required");
    }
}
