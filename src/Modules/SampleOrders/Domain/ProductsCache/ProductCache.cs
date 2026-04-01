using ModularTemplate.Domain.Caching;

namespace Modules.SampleOrders.Domain.ProductsCache;

/// <summary>
/// Cache entity representing products from the SampleSales module.
/// This is a read-only copy maintained via integration events.
/// </summary>
public sealed class ProductCache : ICacheProjection
{
    public int Id { get; set; }

    public Guid RefPublicId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public bool IsActive { get; set; }

    public DateTime LastSyncedAtUtc { get; set; }
}
