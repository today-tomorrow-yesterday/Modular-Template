using Rtl.Core.Domain.Events;

namespace Modules.Sales.Domain.Packages.Events;

public sealed record HomeLineUpdatedDomainEvent : DomainEvent
{
    public int SaleId { get; init; }
    public int PackageId { get; init; }
}
