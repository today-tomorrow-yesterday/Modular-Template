using Modules.Sales.Domain.PartiesCache;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.PartiesCache.UpdatePartyCacheHomeCenter;

// Flow: Sales.UpdatePartyCacheHomeCenterCommand → update Sales.cache.parties.home_center_number
internal sealed class UpdatePartyCacheHomeCenterCommandHandler(
    ICacheWriteScope cacheWriteScope,
    IPartyCacheWriter partyCacheWriter,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<UpdatePartyCacheHomeCenterCommand>
{
    public async Task<Result> Handle(
        UpdatePartyCacheHomeCenterCommand request,
        CancellationToken cancellationToken)
    {
        using var _ = cacheWriteScope.AllowWrites();

        await partyCacheWriter.UpdateHomeCenterNumberAsync(
            request.RefPublicId,
            request.NewHomeCenterNumber,
            dateTimeProvider.UtcNow,
            cancellationToken);

        return Result.Success();
    }
}
