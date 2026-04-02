using ModularTemplate.Domain;

namespace Modules.SampleSales.Domain.OrdersCache;

/// <summary>
/// Read-only repository for order cache data.
/// Write operations are internal and performed only by integration event handlers.
/// </summary>
public interface IOrderCacheRepository : IReadRepository<OrderCache, int>
{
    Task<OrderCache?> GetByRefPublicIdAsync(Guid refPublicId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<OrderCache>> GetByCustomerIdAsync(Guid publicCustomerId, CancellationToken cancellationToken = default);
}
