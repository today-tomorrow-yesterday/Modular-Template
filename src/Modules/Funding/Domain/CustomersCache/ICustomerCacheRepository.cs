using Rtl.Core.Domain;

namespace Modules.Funding.Domain.CustomersCache;

public interface ICustomerCacheRepository : IReadRepository<CustomerCache, int>;
