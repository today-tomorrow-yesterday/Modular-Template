using Microsoft.Extensions.Logging;
using Modules.Customer.IntegrationEvents;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Domain;

namespace Modules.Funding.Presentation.IntegrationEvents;

// Flow: Customer.CustomerNameChangedIntegrationEvent → Funding.UpdateCustomerCacheName
internal sealed class CustomerNameChangedIntegrationEventHandler(
    ICacheWriteScope cacheWriteScope,
    ICustomerCacheWriter customerCacheWriter,
    IDateTimeProvider dateTimeProvider,
    ILogger<CustomerNameChangedIntegrationEventHandler> logger)
    : IntegrationEventHandler<CustomerNameChangedIntegrationEvent>
{
    public override async Task HandleAsync(
        CustomerNameChangedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        using var _ = cacheWriteScope.AllowWrites();

        logger.LogInformation(
            "Processing CustomerNameChanged: PublicCustomerId={PublicCustomerId}",
            integrationEvent.PublicCustomerId);

        await customerCacheWriter.UpdateNameAsync(
            integrationEvent.PublicCustomerId,
            integrationEvent.FirstName ?? string.Empty,
            integrationEvent.LastName ?? string.Empty,
            dateTimeProvider.UtcNow,
            cancellationToken);
    }
}
