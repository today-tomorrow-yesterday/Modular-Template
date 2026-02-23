using Modules.Sales.Domain.PartiesCache;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.PartiesCache.UpdatePartyCacheCoBuyer;

// Flow: Sales.UpdatePartyCacheCoBuyerCommand → update Sales.cache.party_persons co-buyer fields
internal sealed class UpdatePartyCacheCoBuyerCommandHandler(
    ICacheWriteScope cacheWriteScope,
    IPartyCacheWriter partyCacheWriter,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<UpdatePartyCacheCoBuyerCommand>
{
    public async Task<Result> Handle(
        UpdatePartyCacheCoBuyerCommand request,
        CancellationToken cancellationToken)
    {
        using var _ = cacheWriteScope.AllowWrites();

        await partyCacheWriter.UpdateCoBuyerAsync(
            request.RefPartyId,
            request.CoBuyerFirstName,
            request.CoBuyerLastName,
            dateTimeProvider.UtcNow,
            cancellationToken);

        return Result.Success();
    }
}
