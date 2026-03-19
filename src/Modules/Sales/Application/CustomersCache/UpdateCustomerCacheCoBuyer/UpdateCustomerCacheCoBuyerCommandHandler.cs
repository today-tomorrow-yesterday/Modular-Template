using Modules.Sales.Domain.CustomersCache;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.CustomersCache.UpdateCustomerCacheCoBuyer;

// Flow: Sales.UpdateCustomerCacheCoBuyerCommand → update Sales.cache.customers co-buyer fields
internal sealed class UpdateCustomerCacheCoBuyerCommandHandler(
    ICacheWriteScope cacheWriteScope,
    ICustomerCacheWriter customerCacheWriter,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<UpdateCustomerCacheCoBuyerCommand>
{
    public async Task<Result> Handle(
        UpdateCustomerCacheCoBuyerCommand request,
        CancellationToken cancellationToken)
    {
        using var _ = cacheWriteScope.AllowWrites();

        await customerCacheWriter.UpdateCoBuyerAsync(
            request.RefPublicId,
            request.CoBuyerFirstName,
            request.CoBuyerLastName,
            dateTimeProvider.UtcNow,
            cancellationToken);

        return Result.Success();
    }
}
