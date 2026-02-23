using Modules.Sales.Domain.FundingCache;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.FundingCache.UpsertFundingRequestCache;

// Flow: Funding.FundingRequestSubmitted/StatusChanged → Sales.UpsertFundingRequestCacheCommand → upsert cache.funding
// Uses upsert on RefFundingRequestId for idempotency. FundingKeys JSONB is reconstituted from flat event fields.
internal sealed class UpsertFundingRequestCacheCommandHandler(
    ICacheWriteScope cacheWriteScope,
    IFundingRequestCacheWriter cacheWriter,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<UpsertFundingRequestCacheCommand>
{
    public async Task<Result> Handle(
        UpsertFundingRequestCacheCommand request,
        CancellationToken cancellationToken)
    {
        using var _ = cacheWriteScope.AllowWrites();

        request.Cache.LastSyncedAtUtc = dateTimeProvider.UtcNow;

        await cacheWriter.UpsertAsync(request.Cache, cancellationToken);

        return Result.Success();
    }
}
