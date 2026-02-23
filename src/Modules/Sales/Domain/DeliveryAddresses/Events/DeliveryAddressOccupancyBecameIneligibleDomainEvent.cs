using Rtl.Core.Domain.Events;

namespace Modules.Sales.Domain.DeliveryAddresses.Events;

// Occupancy changed to Rental or Investment (not eligible for insurance products).
// Effect: Deletes Insurance and Warranty lines on all draft packages.
public sealed record DeliveryAddressOccupancyBecameIneligibleDomainEvent : DomainEvent
{
    public int SaleId { get; init; }
    public string NewOccupancyType { get; init; } = string.Empty;
}
