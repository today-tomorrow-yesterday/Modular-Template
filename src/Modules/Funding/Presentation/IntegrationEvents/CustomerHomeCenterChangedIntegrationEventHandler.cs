using Microsoft.Extensions.Logging;
using Modules.Customer.IntegrationEvents;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Domain;

namespace Modules.Funding.Presentation.IntegrationEvents;

// Flow: Customer.CustomerHomeCenterChangedIntegrationEvent → Funding.UpdateCustomerCacheHomeCenter
internal sealed class CustomerHomeCenterChangedIntegrationEventHandler(
    ICacheWriteScope cacheWriteScope,
    ICustomerCacheWriter customerCacheWriter,
    IDateTimeProvider dateTimeProvider,
    ILogger<CustomerHomeCenterChangedIntegrationEventHandler> logger)
    : IntegrationEventHandler<CustomerHomeCenterChangedIntegrationEvent>
{
    public override async Task HandleAsync(
        CustomerHomeCenterChangedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        using var _ = cacheWriteScope.AllowWrites();

        logger.LogInformation(
            "Processing CustomerHomeCenterChanged: PublicCustomerId={PublicCustomerId}, NewHC={NewHomeCenterNumber}",
            integrationEvent.PublicCustomerId,
            integrationEvent.NewHomeCenterNumber);

        await customerCacheWriter.UpdateHomeCenterAsync(
            integrationEvent.PublicCustomerId,
            integrationEvent.NewHomeCenterNumber,
            dateTimeProvider.UtcNow,
            cancellationToken);
    }
}
