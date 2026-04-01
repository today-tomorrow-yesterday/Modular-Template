using ModularTemplate.Domain.Events;

namespace Modules.SampleOrders.Domain.Orders.Events;

public sealed record OrderStatusChangedDomainEvent(
    OrderStatus OldStatus,
    OrderStatus NewStatus) : DomainEvent;
