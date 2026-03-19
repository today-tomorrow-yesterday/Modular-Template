using Modules.Sales.Domain.CustomersCache;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.CustomersCache.UpsertCustomerCache;

// Flow: Sales.UpsertCustomerCacheCommand → upsert Sales.cache.customers
internal sealed class UpsertCustomerCacheCommandHandler(
    ICacheWriteScope cacheWriteScope,
    ICustomerCacheWriter customerCacheWriter,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<UpsertCustomerCacheCommand>
{
    public async Task<Result> Handle(
        UpsertCustomerCacheCommand request,
        CancellationToken cancellationToken)
    {
        using var _ = cacheWriteScope.AllowWrites();

        request.CustomerCache.LastSyncedAtUtc = dateTimeProvider.UtcNow;

        await customerCacheWriter.UpsertAsync(
            request.CustomerCache,
            cancellationToken);

        return Result.Success();
    }
}
