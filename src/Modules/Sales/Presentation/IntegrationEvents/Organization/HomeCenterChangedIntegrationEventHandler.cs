using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Organization.IntegrationEvents;
using Modules.Sales.Application.RetailLocations.UpsertRetailLocation;
using Rtl.Core.Application.EventBus;

namespace Modules.Sales.Presentation.IntegrationEvents.Organization;

// Flow: Organization.HomeCenterChanged (EventBridge) → Send UpsertRetailLocationCommand → Upsert sales.retail_locations
// Single-write to RetailLocation entity. cache.home_centers is REMOVED — retail_locations is the sole Organization data target.
internal sealed class HomeCenterChangedIntegrationEventHandler(
    ISender sender,
    ILogger<HomeCenterChangedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<HomeCenterChangedIntegrationEvent>
{
    public async Task HandleAsync(
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
            new UpsertRetailLocationCommand(
                integrationEvent.HomeCenterNumber,
                integrationEvent.LotName,
                integrationEvent.StateCode,
                integrationEvent.Zip,
                integrationEvent.IsActive),
            cancellationToken);
    }

    public Task HandleAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
        => HandleAsync((HomeCenterChangedIntegrationEvent)integrationEvent, cancellationToken);
}
