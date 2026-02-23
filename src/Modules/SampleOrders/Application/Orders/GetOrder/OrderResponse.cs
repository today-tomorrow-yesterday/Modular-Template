using Modules.SampleOrders.Domain.Orders;

namespace Modules.SampleOrders.Application.Orders.GetOrder;

public sealed record OrderResponse(
    int Id,
    int CustomerId,
    IReadOnlyCollection<OrderLineResponse> Lines,
    decimal TotalPrice,
    string Currency,
    OrderStatus Status,
    DateTime OrderedAtUtc,
    DateTime CreatedAtUtc,
    Guid CreatedByUserId,
    DateTime? ModifiedAtUtc,
    Guid? ModifiedByUserId);

public sealed record OrderLineResponse(
    int Id,
    int ProductId,
    int Quantity,
    decimal UnitPrice,
    string Currency,
    decimal LineTotal);
