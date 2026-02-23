using Rtl.Core.Application.EventBus;

namespace Modules.SampleOrders.IntegrationEvents;

/// <summary>
/// Integration event published when an order status is changed.
/// Other modules (e.g., Sales) can subscribe to update their OrderCache.Status.
/// </summary>
public sealed record OrderStatusChangedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    int OrderId,
    string NewStatus) : IntegrationEvent(Id, OccurredOnUtc);
