using Rtl.Core.Domain.Events;

namespace Modules.Sales.Domain.DeliveryAddresses.Events;

// Effect: Clears state-specific tax question answers on all draft packages for the sale.
public sealed record DeliveryAddressStateChangedDomainEvent : DomainEvent
{
    public int SaleId { get; init; }
    public string? OldState { get; init; }
    public string? NewState { get; init; }
}
