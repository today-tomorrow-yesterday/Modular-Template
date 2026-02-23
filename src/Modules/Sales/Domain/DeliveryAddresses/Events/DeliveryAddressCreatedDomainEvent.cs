using Rtl.Core.Domain.Events;

namespace Modules.Sales.Domain.DeliveryAddresses.Events;

// Integration event → Customer module. Same payload as DeliveryAddressChanged.
public sealed record DeliveryAddressCreatedDomainEvent : DomainEvent
{
    public int SaleId { get; init; }
    public int DeliveryAddressId { get; init; }
}
