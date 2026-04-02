using FluentValidation;

namespace Modules.SampleOrders.Application.Orders.PlaceOrder;

internal sealed class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderCommandValidator()
    {
        RuleFor(x => x.PublicCustomerId)
            .NotEmpty()
            .WithMessage("PublicCustomerId is required");

        RuleFor(x => x.ProductCacheId)
            .GreaterThan(0)
            .WithMessage("ProductCacheId is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than zero");
    }
}
