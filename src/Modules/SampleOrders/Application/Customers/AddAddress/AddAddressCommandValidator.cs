using FluentValidation;

namespace Modules.SampleOrders.Application.Customers.AddAddress;

internal sealed class AddAddressCommandValidator : AbstractValidator<AddAddressCommand>
{
    public AddAddressCommandValidator()
    {
        RuleFor(x => x.PublicCustomerId)
            .NotEmpty()
            .WithMessage("PublicCustomerId is required");

        RuleFor(x => x.AddressLine1)
            .NotEmpty()
            .WithMessage("Address line 1 is required")
            .MaximumLength(200)
            .WithMessage("Address line 1 cannot exceed 200 characters");

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("City is required")
            .MaximumLength(100)
            .WithMessage("City cannot exceed 100 characters");

        RuleFor(x => x.State)
            .NotEmpty()
            .WithMessage("State is required")
            .MaximumLength(2)
            .WithMessage("State cannot exceed 2 characters");

        RuleFor(x => x.PostalCode)
            .NotEmpty()
            .WithMessage("Postal code is required")
            .MaximumLength(20)
            .WithMessage("Postal code cannot exceed 20 characters");
    }
}
