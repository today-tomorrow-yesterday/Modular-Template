using Modules.Sales.Domain.PartiesCache;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.PartiesCache.UpdatePartyCacheLifecycle;

// Flow: Sales.UpdatePartyCacheLifecycleCommand → update Sales.cache.parties.lifecycle_stage
internal sealed class UpdatePartyCacheLifecycleCommandHandler(
    ICacheWriteScope cacheWriteScope,
    IPartyCacheWriter partyCacheWriter,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<UpdatePartyCacheLifecycleCommand>
{
    public async Task<Result> Handle(
        UpdatePartyCacheLifecycleCommand request,
        CancellationToken cancellationToken)
    {
        using var _ = cacheWriteScope.AllowWrites();

        await partyCacheWriter.UpdateLifecycleStageAsync(
            request.RefPartyId,
            request.NewLifecycleStage,
            dateTimeProvider.UtcNow,
            cancellationToken);

        return Result.Success();
    }
}
