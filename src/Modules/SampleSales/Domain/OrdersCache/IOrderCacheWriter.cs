namespace Modules.SampleSales.Domain.OrdersCache;

public interface IOrderCacheWriter
{
    Task UpsertAsync(OrderCache orderCache, CancellationToken cancellationToken = default);
}
