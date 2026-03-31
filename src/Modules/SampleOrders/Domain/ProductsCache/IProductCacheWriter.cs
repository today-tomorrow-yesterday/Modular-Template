namespace Modules.SampleOrders.Domain.ProductsCache;

public interface IProductCacheWriter
{
    Task UpsertAsync(ProductCache productCache, CancellationToken cancellationToken = default);
}
