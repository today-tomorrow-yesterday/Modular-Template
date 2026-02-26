using Rtl.Core.Domain.Events;

namespace Modules.Sales.Domain.Packages.Land;

// Consumer: Tax error clearing handler.
public sealed record LandLineUpdatedDomainEvent : DomainEvent
{
    public int PackageId { get; init; }
    public int SaleId { get; init; }
}
