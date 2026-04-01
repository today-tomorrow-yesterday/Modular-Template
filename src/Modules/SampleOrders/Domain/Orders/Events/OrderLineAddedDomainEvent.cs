using ModularTemplate.Domain.Events;

namespace Modules.SampleOrders.Domain.Orders.Events;

public sealed record OrderLineAddedDomainEvent(int LineId) : DomainEvent;
