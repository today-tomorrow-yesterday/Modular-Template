using Rtl.Core.Domain;

namespace Modules.SampleOrders.Domain.ProductsCache;

/// <summary>
/// Read-only repository for product cache data.
/// Write operations are internal and performed only by integration event handlers.
/// </summary>
public interface IProductCacheRepository : IReadRepository<ProductCache, int>
{
    Task<IReadOnlyCollection<ProductCache>> GetActiveProductsAsync(CancellationToken cancellationToken = default);
}
