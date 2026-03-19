using Modules.Sales.Domain.CustomersCache;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.CustomersCache.UpdateCustomerCacheHomeCenter;

// Flow: Sales.UpdateCustomerCacheHomeCenterCommand → update Sales.cache.customers.home_center_number
internal sealed class UpdateCustomerCacheHomeCenterCommandHandler(
    ICacheWriteScope cacheWriteScope,
    ICustomerCacheWriter customerCacheWriter,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<UpdateCustomerCacheHomeCenterCommand>
{
    public async Task<Result> Handle(
        UpdateCustomerCacheHomeCenterCommand request,
        CancellationToken cancellationToken)
    {
        using var _ = cacheWriteScope.AllowWrites();

        await customerCacheWriter.UpdateHomeCenterNumberAsync(
            request.RefPublicId,
            request.NewHomeCenterNumber,
            dateTimeProvider.UtcNow,
            cancellationToken);

        return Result.Success();
    }
}
