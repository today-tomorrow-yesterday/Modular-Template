using Modules.Sales.Domain;
using Modules.Sales.Domain.RetailLocationCache;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.RetailLocationCache.UpsertRetailLocationCache;

// Flow: Organization.HomeCenterChanged -> Sales.UpsertRetailLocationCacheCommand -> upsert cache.retail_location_cache
// Single-write to RetailLocationCache — the sole Organization data target in the Sales module.
internal sealed class UpsertRetailLocationCacheCommandHandler(
    IRetailLocationCacheRepository retailLocationCacheRepository,
    ICacheWriteScope cacheWriteScope,
    IUnitOfWork<ISalesModule> unitOfWork)
    : ICommandHandler<UpsertRetailLocationCacheCommand>
{
    public async Task<Result> Handle(
        UpsertRetailLocationCacheCommand request,
        CancellationToken cancellationToken)
    {
        using var _ = cacheWriteScope.AllowWrites();

        var existing = await retailLocationCacheRepository.GetByHomeCenterNumberAsync(
            request.HomeCenterNumber, cancellationToken);

        if (existing is null)
        {
            var retailLocation = Domain.RetailLocationCache.RetailLocationCache.CreateHomeCenter(
                request.HomeCenterNumber,
                request.Name,
                request.StateCode,
                request.Zip,
                request.IsActive);

            retailLocationCacheRepository.Add(retailLocation);
        }
        else
        {
            existing.UpdateFromHomeCenterChanged(
                request.Name,
                request.StateCode,
                request.Zip,
                request.IsActive,
                organizationMetadata: null);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
