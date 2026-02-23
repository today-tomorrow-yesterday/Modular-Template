using Modules.Sales.Domain.PartiesCache;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.PartiesCache.UpdatePartyCacheSalesAssignments;

// Flow: Sales.UpdatePartyCacheSalesAssignmentsCommand → update Sales.cache.party_persons sales person fields
internal sealed class UpdatePartyCacheSalesAssignmentsCommandHandler(
    ICacheWriteScope cacheWriteScope,
    IPartyCacheWriter partyCacheWriter,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<UpdatePartyCacheSalesAssignmentsCommand>
{
    public async Task<Result> Handle(
        UpdatePartyCacheSalesAssignmentsCommand request,
        CancellationToken cancellationToken)
    {
        using var _ = cacheWriteScope.AllowWrites();

        await partyCacheWriter.UpdateSalesAssignmentsAsync(
            request.RefPartyId,
            request.PrimaryFederatedId,
            request.PrimaryFirstName,
            request.PrimaryLastName,
            request.SecondaryFederatedId,
            request.SecondaryFirstName,
            request.SecondaryLastName,
            dateTimeProvider.UtcNow,
            cancellationToken);

        return Result.Success();
    }
}
