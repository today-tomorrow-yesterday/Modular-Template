using FluentValidation;

namespace Modules.Customer.Application.Customers.OnboardCustomerFromLoan;

internal sealed class OnboardCustomerFromLoanCommandValidator : AbstractValidator<OnboardCustomerFromLoanCommand>
{
    public OnboardCustomerFromLoanCommandValidator()
    {
        RuleFor(x => x.LoanId)
            .NotEmpty()
            .WithMessage("LoanId is required");

        RuleFor(x => x.HomeCenterNumber)
            .GreaterThan(0)
            .WithMessage("HomeCenterNumber is required");
    }
}
