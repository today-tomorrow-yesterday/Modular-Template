namespace Modules.Sales.Domain.FundingCache;

public interface IFundingRequestCacheWriter
{
    Task UpsertAsync(FundingRequestCache cache, CancellationToken cancellationToken = default);
}
