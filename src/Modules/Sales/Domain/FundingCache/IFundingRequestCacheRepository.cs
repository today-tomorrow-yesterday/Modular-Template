using Rtl.Core.Domain;

namespace Modules.Sales.Domain.FundingCache;

public interface IFundingRequestCacheRepository : IReadRepository<FundingRequestCache, int>
{
    Task<FundingRequestCache?> GetByPackageIdAsync(int packageId, CancellationToken cancellationToken = default);
}
