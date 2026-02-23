using Modules.Inventory.Domain.SaleSummariesCache;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;
using Rtl.Core.Domain.Results;

namespace Modules.Inventory.Application.SaleSummariesCache.UpsertSaleSummaryCache;

// Flow: Sales.SaleSummaryChanged → upsert Inventory.cache.sale_summaries
internal sealed class UpsertSaleSummaryCacheCommandHandler(
    ICacheWriteScope cacheWriteScope,
    ISaleSummaryCacheWriter cacheWriter,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<UpsertSaleSummaryCacheCommand>
{
    public async Task<Result> Handle(
        UpsertSaleSummaryCacheCommand request,
        CancellationToken cancellationToken)
    {
        using var _ = cacheWriteScope.AllowWrites();

        request.Cache.LastSyncedAtUtc = dateTimeProvider.UtcNow;

        await cacheWriter.UpsertAsync(request.Cache, cancellationToken);

        return Result.Success();
    }
}
