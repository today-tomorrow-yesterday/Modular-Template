using FluentValidation;

namespace Modules.SampleOrders.Application.Orders.UpdateOrderStatus;

internal sealed class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.PublicOrderId)
            .NotEmpty()
            .WithMessage("PublicOrderId is required");

        RuleFor(x => x.NewStatus)
            .IsInEnum()
            .WithMessage("Invalid order status");
    }
}
