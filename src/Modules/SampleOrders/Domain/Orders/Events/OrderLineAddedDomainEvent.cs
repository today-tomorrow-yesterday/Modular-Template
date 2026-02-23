using Rtl.Core.Domain.Events;

namespace Modules.SampleOrders.Domain.Orders.Events;

public sealed record OrderLineAddedDomainEvent(
    int ProductId,
    int Quantity) : DomainEvent;
