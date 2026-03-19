using Modules.Sales.Domain.CustomersCache;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.CustomersCache.UpdateCustomerCacheMailingAddress;

// Flow: Sales.UpdateCustomerCacheMailingAddressCommand → update Sales.cache.customers sync timestamp (no address columns cached yet)
internal sealed class UpdateCustomerCacheMailingAddressCommandHandler(
    ICacheWriteScope cacheWriteScope,
    ICustomerCacheWriter customerCacheWriter,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<UpdateCustomerCacheMailingAddressCommand>
{
    public async Task<Result> Handle(
        UpdateCustomerCacheMailingAddressCommand request,
        CancellationToken cancellationToken)
    {
        using var _ = cacheWriteScope.AllowWrites();

        await customerCacheWriter.UpdateMailingAddressAsync(
            request.RefPublicId,
            dateTimeProvider.UtcNow,
            cancellationToken);

        return Result.Success();
    }
}
