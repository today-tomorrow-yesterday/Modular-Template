using FluentValidation;

namespace Modules.Customer.Application.Customers.SyncCustomerFromCrm;

internal sealed class SyncCustomerFromCrmCommandValidator : AbstractValidator<SyncCustomerFromCrmCommand>
{
    public SyncCustomerFromCrmCommandValidator()
    {
        RuleFor(x => x.CrmCustomerId)
            .GreaterThan(0)
            .WithMessage("CrmCustomerId is required");

        RuleFor(x => x.HomeCenterNumber)
            .GreaterThan(0)
            .WithMessage("HomeCenterNumber is required");

        RuleFor(x => x.LifecycleStage)
            .IsInEnum()
            .WithMessage("LifecycleStage must be Lead, Opportunity, or Customer");
    }
}
