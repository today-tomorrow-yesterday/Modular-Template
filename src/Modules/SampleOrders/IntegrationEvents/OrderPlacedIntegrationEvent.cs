using ModularTemplate.Application.EventBus;

namespace Modules.SampleOrders.IntegrationEvents;

/// <summary>
/// Integration event published when a new order is placed.
/// Other modules (e.g., Sales) can subscribe to sync their OrderCache.
/// </summary>
[EventDetailType("mt.sampleOrders.orderPlaced")]
public sealed record OrderPlacedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid PublicOrderId,
    Guid PublicCustomerId,
    IReadOnlyCollection<OrderLineDto> Lines,
    decimal TotalPrice,
    string Currency,
    string Status,
    DateTime OrderedAtUtc) : IntegrationEvent(Id, OccurredOnUtc);

public sealed record OrderLineDto(
    Guid PublicProductId,
    int Quantity,
    decimal UnitPrice,
    string Currency);
