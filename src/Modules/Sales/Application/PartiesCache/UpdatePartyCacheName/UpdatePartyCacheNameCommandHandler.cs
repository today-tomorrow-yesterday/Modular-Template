using Modules.Sales.Domain.PartiesCache;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.PartiesCache.UpdatePartyCacheName;

// Flow: Sales.UpdatePartyCacheNameCommand → update Sales.cache.parties display_name + person/org name fields
internal sealed class UpdatePartyCacheNameCommandHandler(
    ICacheWriteScope cacheWriteScope,
    IPartyCacheWriter partyCacheWriter,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<UpdatePartyCacheNameCommand>
{
    public async Task<Result> Handle(
        UpdatePartyCacheNameCommand request,
        CancellationToken cancellationToken)
    {
        using var _ = cacheWriteScope.AllowWrites();

        await partyCacheWriter.UpdateNameAsync(
            request.RefPartyId,
            request.PartyType,
            request.DisplayName,
            request.FirstName,
            request.MiddleName,
            request.LastName,
            request.OrganizationName,
            dateTimeProvider.UtcNow,
            cancellationToken);

        return Result.Success();
    }
}
