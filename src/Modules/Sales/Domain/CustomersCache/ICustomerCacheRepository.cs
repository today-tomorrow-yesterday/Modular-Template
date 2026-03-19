using Rtl.Core.Domain;

namespace Modules.Sales.Domain.CustomersCache;

public interface ICustomerCacheRepository : IReadRepository<CustomerCache, int>
{
    Task<CustomerCache?> GetByRefPublicIdAsync(Guid refPublicId, CancellationToken cancellationToken = default);
}
