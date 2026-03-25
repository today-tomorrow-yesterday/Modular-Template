using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Organization.IntegrationEvents;
using Modules.Sales.Application.RetailLocationCache.UpsertRetailLocationCache;
using Rtl.Core.Application.EventBus;

namespace Modules.Sales.Presentation.IntegrationEvents.Organization;

// Flow: Organization.HomeCenterChanged (EventBridge) → Send UpsertRetailLocationCacheCommand → Upsert cache.retail_location_cache
// Single-write to RetailLocationCache entity — the sole Organization data target in the Sales module.
internal sealed class HomeCenterChangedIntegrationEventHandler(
    ISender sender,
    ILogger<HomeCenterChangedIntegrationEventHandler> logger)
    : IntegrationEventHandler<HomeCenterChangedIntegrationEvent>
{
    public override async Task HandleAsync(
        HomeCenterChangedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing HomeCenterChanged: HC={HomeCenterNumber}, Name={LotName}, State={StateCode}, Active={IsActive}",
            integrationEvent.HomeCenterNumber,
            integrationEvent.LotName,
            integrationEvent.StateCode,
            integrationEvent.IsActive);

        await sender.Send(
            new UpsertRetailLocationCacheCommand(
                integrationEvent.HomeCenterNumber,
                integrationEvent.LotName,
                integrationEvent.StateCode,
                integrationEvent.Zip,
                integrationEvent.IsActive),
            cancellationToken);
    }
}
