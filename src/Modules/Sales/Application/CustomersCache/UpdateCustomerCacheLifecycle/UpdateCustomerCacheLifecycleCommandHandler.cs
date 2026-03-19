using Modules.Sales.Domain.CustomersCache;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.CustomersCache.UpdateCustomerCacheLifecycle;

// Flow: Sales.UpdateCustomerCacheLifecycleCommand → update Sales.cache.customers.lifecycle_stage
internal sealed class UpdateCustomerCacheLifecycleCommandHandler(
    ICacheWriteScope cacheWriteScope,
    ICustomerCacheWriter customerCacheWriter,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<UpdateCustomerCacheLifecycleCommand>
{
    public async Task<Result> Handle(
        UpdateCustomerCacheLifecycleCommand request,
        CancellationToken cancellationToken)
    {
        using var _ = cacheWriteScope.AllowWrites();

        await customerCacheWriter.UpdateLifecycleStageAsync(
            request.RefPublicId,
            request.NewLifecycleStage,
            dateTimeProvider.UtcNow,
            cancellationToken);

        return Result.Success();
    }
}
