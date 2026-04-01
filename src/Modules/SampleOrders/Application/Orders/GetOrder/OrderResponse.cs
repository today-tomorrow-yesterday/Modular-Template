using Modules.SampleOrders.Domain.Orders;

namespace Modules.SampleOrders.Application.Orders.GetOrder;

public sealed record OrderResponse(
    Guid PublicId,
    IReadOnlyCollection<OrderLineResponse> Lines,
    decimal TotalPrice,
    string Currency,
    OrderStatus Status,
    DateTime OrderedAtUtc,
    ShippingAddressResponse? ShippingAddress,
    DateTime CreatedAtUtc,
    Guid CreatedByUserId,
    DateTime? ModifiedAtUtc,
    Guid? ModifiedByUserId);

public sealed record OrderLineResponse(
    string LineType,
    int Quantity,
    decimal UnitPrice,
    string Currency,
    decimal LineTotal);

public sealed record ShippingAddressResponse(
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? State,
    string? PostalCode,
    string? Country);
