using ModularTemplate.Domain.Caching;

namespace Modules.SampleSales.Domain.OrdersCache;

/// <summary>
/// Cache entity representing orders from the SampleOrders module.
/// This is a read-only copy maintained via integration events.
/// </summary>
public sealed class OrderCache : ICacheProjection
{
    public int Id { get; set; }

    public Guid RefPublicId { get; set; }

    public Guid RefPublicCustomerId { get; set; }

    public decimal TotalPrice { get; set; }

    public string Currency { get; set; } = "USD";

    public string Status { get; set; } = string.Empty;

    public DateTime OrderedAtUtc { get; set; }

    public DateTime LastSyncedAtUtc { get; set; }
}
