using Rtl.Core.Domain.Events;

namespace Modules.Sales.Domain.Packages.Home;

// Consumers: Insurance and Warranty handlers (re-check eligibility).
public sealed record HomeLineUpdatedDomainEvent : DomainEvent
{
    public int PackageId { get; init; }
    public int SaleId { get; init; }
}
