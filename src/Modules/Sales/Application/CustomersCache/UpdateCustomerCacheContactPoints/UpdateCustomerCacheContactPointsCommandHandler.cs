using Modules.Sales.Domain.CustomersCache;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.CustomersCache.UpdateCustomerCacheContactPoints;

// Flow: Sales.UpdateCustomerCacheContactPointsCommand → update Sales.cache.customers email + phone
internal sealed class UpdateCustomerCacheContactPointsCommandHandler(
    ICacheWriteScope cacheWriteScope,
    ICustomerCacheWriter customerCacheWriter,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<UpdateCustomerCacheContactPointsCommand>
{
    public async Task<Result> Handle(
        UpdateCustomerCacheContactPointsCommand request,
        CancellationToken cancellationToken)
    {
        using var _ = cacheWriteScope.AllowWrites();

        await customerCacheWriter.UpdateContactPointsAsync(
            request.RefPublicId,
            request.Email,
            request.Phone,
            dateTimeProvider.UtcNow,
            cancellationToken);

        return Result.Success();
    }
}
