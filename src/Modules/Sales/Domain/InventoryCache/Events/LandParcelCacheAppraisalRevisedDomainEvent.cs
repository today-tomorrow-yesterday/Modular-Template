using Rtl.Core.Domain.Events;

namespace Modules.Sales.Domain.InventoryCache.Events;

public sealed record LandParcelCacheAppraisalRevisedDomainEvent : DomainEvent
{
    public int LandParcelCacheId { get; init; }
    public string StockNumber { get; init; } = string.Empty;
}
