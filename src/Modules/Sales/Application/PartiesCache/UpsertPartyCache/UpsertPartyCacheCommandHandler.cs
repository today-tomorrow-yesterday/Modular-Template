using Modules.Sales.Domain.PartiesCache;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.PartiesCache.UpsertPartyCache;

// Flow: Sales.UpsertPartyCacheCommand → upsert Sales.cache.parties
internal sealed class UpsertPartyCacheCommandHandler(
    ICacheWriteScope cacheWriteScope,
    IPartyCacheWriter partyCacheWriter,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<UpsertPartyCacheCommand>
{
    public async Task<Result> Handle(
        UpsertPartyCacheCommand request,
        CancellationToken cancellationToken)
    {
        using var _ = cacheWriteScope.AllowWrites();

        request.PartyCache.LastSyncedAtUtc = dateTimeProvider.UtcNow;

        await partyCacheWriter.UpsertAsync(
            request.PartyCache,
            request.PersonCache,
            request.OrganizationCache,
            cancellationToken);

        return Result.Success();
    }
}
