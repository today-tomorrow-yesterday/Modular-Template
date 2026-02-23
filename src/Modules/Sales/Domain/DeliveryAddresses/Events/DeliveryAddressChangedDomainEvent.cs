using Rtl.Core.Domain.Events;

namespace Modules.Sales.Domain.DeliveryAddresses.Events;

// Integration event → Customer module.
public sealed record DeliveryAddressChangedDomainEvent : DomainEvent
{
    public int SaleId { get; init; }
    public int DeliveryAddressId { get; init; }
}
