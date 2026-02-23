using Modules.Sales.Domain.PartiesCache;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.PartiesCache.UpdatePartyCacheContactPoints;

// Flow: Sales.UpdatePartyCacheContactPointsCommand → update Sales.cache.party_persons email + phone
internal sealed class UpdatePartyCacheContactPointsCommandHandler(
    ICacheWriteScope cacheWriteScope,
    IPartyCacheWriter partyCacheWriter,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<UpdatePartyCacheContactPointsCommand>
{
    public async Task<Result> Handle(
        UpdatePartyCacheContactPointsCommand request,
        CancellationToken cancellationToken)
    {
        using var _ = cacheWriteScope.AllowWrites();

        await partyCacheWriter.UpdateContactPointsAsync(
            request.RefPartyId,
            request.Email,
            request.Phone,
            dateTimeProvider.UtcNow,
            cancellationToken);

        return Result.Success();
    }
}
