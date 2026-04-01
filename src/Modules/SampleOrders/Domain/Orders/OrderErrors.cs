using ModularTemplate.Domain.Results;

namespace Modules.SampleOrders.Domain.Orders;

public static class OrderErrors
{
    public static Error NotFound(int orderId) =>
        Error.NotFound("Orders.NotFound", $"The order with ID '{orderId}' was not found.");

    public static readonly Error ProductNotFound =
        Error.Validation("Orders.ProductNotFound", "The product for this order was not found.");

    public static readonly Error QuantityInvalid =
        Error.Validation("Orders.QuantityInvalid", "The order quantity must be greater than zero.");

    public static readonly Error InvalidStatusTransition =
        Error.Validation("Orders.InvalidStatusTransition", "The status transition is not allowed.");

    public static readonly Error CustomerRequired =
        Error.Validation("Orders.CustomerRequired", "A customer is required to place an order.");

    public static readonly Error CannotModifyNonPendingOrder =
        Error.Validation("Orders.CannotModifyNonPendingOrder", "Only pending orders can be modified.");

    public static readonly Error LineNotFound =
        Error.NotFound("Orders.LineNotFound", "The order line was not found.");
}
