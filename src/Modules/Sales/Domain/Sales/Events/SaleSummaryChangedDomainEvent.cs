using Rtl.Core.Domain.Events;

namespace Modules.Sales.Domain.Sales.Events;

// Integration event → Inventory module (ECST). Includes status, customer, retail location data.
public sealed record SaleSummaryChangedDomainEvent : DomainEvent
{
    public int SaleId { get; init; }
}
