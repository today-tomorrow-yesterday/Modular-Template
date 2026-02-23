using Rtl.Core.Domain.Events;

namespace Modules.Sales.Domain.InventoryCache.Events;

public sealed record OnLotHomeCachePriceRevisedDomainEvent : DomainEvent
{
    public int OnLotHomeCacheId { get; init; }
    public string StockNumber { get; init; } = string.Empty;
}
