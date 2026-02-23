using Modules.Inventory.Domain.HomeCentersCache;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;
using Rtl.Core.Domain.Results;

namespace Modules.Inventory.Application.HomeCentersCache.UpsertHomeCenterCache;

// Flow: Organization.HomeCenterChanged → upsert Inventory.cache.home_centers
internal sealed class UpsertHomeCenterCacheCommandHandler(
    ICacheWriteScope cacheWriteScope,
    IHomeCenterCacheWriter cacheWriter,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<UpsertHomeCenterCacheCommand>
{
    public async Task<Result> Handle(
        UpsertHomeCenterCacheCommand request,
        CancellationToken cancellationToken)
    {
        using var _ = cacheWriteScope.AllowWrites();

        request.Cache.LastSyncedAtUtc = dateTimeProvider.UtcNow;

        await cacheWriter.UpsertAsync(request.Cache, cancellationToken);

        return Result.Success();
    }
}
