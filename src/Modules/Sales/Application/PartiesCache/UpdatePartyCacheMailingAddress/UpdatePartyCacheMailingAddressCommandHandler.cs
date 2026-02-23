using Modules.Sales.Domain.PartiesCache;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.PartiesCache.UpdatePartyCacheMailingAddress;

// Flow: Sales.UpdatePartyCacheMailingAddressCommand → update Sales.cache.parties sync timestamp (no address columns cached yet)
internal sealed class UpdatePartyCacheMailingAddressCommandHandler(
    ICacheWriteScope cacheWriteScope,
    IPartyCacheWriter partyCacheWriter,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<UpdatePartyCacheMailingAddressCommand>
{
    public async Task<Result> Handle(
        UpdatePartyCacheMailingAddressCommand request,
        CancellationToken cancellationToken)
    {
        using var _ = cacheWriteScope.AllowWrites();

        await partyCacheWriter.UpdateMailingAddressAsync(
            request.RefPartyId,
            dateTimeProvider.UtcNow,
            cancellationToken);

        return Result.Success();
    }
}
