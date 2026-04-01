using FluentValidation;

namespace Modules.SampleOrders.Application.Customers.AddContact;

internal sealed class AddContactCommandValidator : AbstractValidator<AddContactCommand>
{
    public AddContactCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0)
            .WithMessage("CustomerId is required");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invalid contact type");

        RuleFor(x => x.Value)
            .NotEmpty()
            .WithMessage("Contact value is required")
            .MaximumLength(200)
            .WithMessage("Contact value cannot exceed 200 characters");
    }
}
