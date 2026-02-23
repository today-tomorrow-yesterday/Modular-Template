using Rtl.Core.Domain.Events;

namespace Modules.Sales.Domain.DeliveryAddresses.Events;

// Location fields: City, State, PostalCode, County, IsWithinCityLimits.
// Effect: Clears TaxDetails.TaxItems, removes Use Tax (Cat 9/21),
// sets MustRecalculateTaxes on all draft packages. Tax jurisdiction is delivery-location-based.
public sealed record DeliveryAddressLocationChangedDomainEvent : DomainEvent
{
    public int SaleId { get; init; }
}
