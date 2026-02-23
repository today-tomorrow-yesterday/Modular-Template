using Modules.Sales.Domain.AuthorizedUsersCache;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.AuthorizedUsersCache.UpsertAuthorizedUserCache;

// Flow: Organization.UserAccessGranted/Changed → Sales.UpsertAuthorizedUserCacheCommand → upsert cache.authorized_users
// Uses upsert on RefUserId for idempotency (duplicate events from EventBridge are safe).
internal sealed class UpsertAuthorizedUserCacheCommandHandler(
    ICacheWriteScope cacheWriteScope,
    IAuthorizedUserCacheWriter cacheWriter,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<UpsertAuthorizedUserCacheCommand>
{
    public async Task<Result> Handle(
        UpsertAuthorizedUserCacheCommand request,
        CancellationToken cancellationToken)
    {
        using var _ = cacheWriteScope.AllowWrites();

        request.Cache.LastSyncedAtUtc = dateTimeProvider.UtcNow;

        await cacheWriter.UpsertAsync(request.Cache, cancellationToken);

        return Result.Success();
    }
}
