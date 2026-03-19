using Modules.Sales.Domain.CustomersCache;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.CustomersCache.UpdateCustomerCacheSalesAssignments;

// Flow: Sales.UpdateCustomerCacheSalesAssignmentsCommand → update Sales.cache.customers sales person fields
internal sealed class UpdateCustomerCacheSalesAssignmentsCommandHandler(
    ICacheWriteScope cacheWriteScope,
    ICustomerCacheWriter customerCacheWriter,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<UpdateCustomerCacheSalesAssignmentsCommand>
{
    public async Task<Result> Handle(
        UpdateCustomerCacheSalesAssignmentsCommand request,
        CancellationToken cancellationToken)
    {
        using var _ = cacheWriteScope.AllowWrites();

        await customerCacheWriter.UpdateSalesAssignmentsAsync(
            request.RefPublicId,
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
