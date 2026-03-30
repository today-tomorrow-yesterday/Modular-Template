using Rtl.Core.Domain.Events;

namespace Modules.Sales.Domain.Packages.Events;

// Raised when an inventory product (on-lot home or land parcel) referenced by a package line
// is removed from the Inventory module's catalog.
public sealed record ProductRemovedFromInventoryDomainEvent : DomainEvent
{
    public int PackageId { get; init; }
    public Guid PackagePublicId { get; init; }
    public int SaleId { get; init; }
    public int PackageLineId { get; init; }
    public string ProductType { get; init; } = string.Empty;
    public string? StockNumber { get; init; }
}
