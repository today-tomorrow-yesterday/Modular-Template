using Modules.Sales.Domain.CustomersCache;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.CustomersCache.UpdateCustomerCacheName;

// Flow: Sales.UpdateCustomerCacheNameCommand → update Sales.cache.customers display_name + name fields
internal sealed class UpdateCustomerCacheNameCommandHandler(
    ICacheWriteScope cacheWriteScope,
    ICustomerCacheWriter customerCacheWriter,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<UpdateCustomerCacheNameCommand>
{
    public async Task<Result> Handle(
        UpdateCustomerCacheNameCommand request,
        CancellationToken cancellationToken)
    {
        using var _ = cacheWriteScope.AllowWrites();

        await customerCacheWriter.UpdateNameAsync(
            request.RefPublicId,
            request.DisplayName,
            request.FirstName,
            request.MiddleName,
            request.LastName,
            dateTimeProvider.UtcNow,
            cancellationToken);

        return Result.Success();
    }
}
