using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Customer.IntegrationEvents;
using Modules.Sales.Application.PartiesCache.UpdatePartyCacheLifecycle;
using Modules.Sales.Domain.PartiesCache;
using Rtl.Core.Application.EventBus;

namespace Modules.Sales.Presentation.IntegrationEvents.Customer;

// Flow: Customer.PartyLifecycleAdvancedIntegrationEvent → Sales.UpdatePartyCacheLifecycleCommand
internal sealed class PartyLifecycleAdvancedIntegrationEventHandler(
    ISender sender,
    ILogger<PartyLifecycleAdvancedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<PartyLifecycleAdvancedIntegrationEvent>
{
    public async Task HandleAsync(
        PartyLifecycleAdvancedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing PartyLifecycleAdvanced: PartyId={PartyId}, NewStage={NewStage}",
            integrationEvent.PartyId,
            integrationEvent.NewLifecycleStage);

        await sender.Send(
            new UpdatePartyCacheLifecycleCommand(
                integrationEvent.PartyId,
                Enum.Parse<LifecycleStage>(integrationEvent.NewLifecycleStage)),
            cancellationToken);
    }

    public Task HandleAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
        => HandleAsync((PartyLifecycleAdvancedIntegrationEvent)integrationEvent, cancellationToken);
}
