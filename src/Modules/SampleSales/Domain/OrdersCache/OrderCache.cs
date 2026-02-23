using Rtl.Core.Domain.Caching;

namespace Modules.SampleSales.Domain.OrdersCache;

/// <summary>
/// Cache entity representing orders from the Orders module.
/// This is a read-only copy maintained via integration events.
/// No audit fields or soft delete - cache entities are simple data copies.
/// </summary>
public sealed class OrderCache : ICacheProjection
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public decimal TotalPrice { get; set; }

    public string Currency { get; set; } = "USD";

    public string Status { get; set; } = string.Empty;

    public DateTime OrderedAtUtc { get; set; }

    public DateTime LastSyncedAtUtc { get; set; }
}
