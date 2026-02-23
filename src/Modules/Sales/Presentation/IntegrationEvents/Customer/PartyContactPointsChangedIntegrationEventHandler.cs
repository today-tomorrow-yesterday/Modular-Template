using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Customer.IntegrationEvents;
using Modules.Sales.Application.PartiesCache.UpdatePartyCacheContactPoints;
using Rtl.Core.Application.EventBus;

namespace Modules.Sales.Presentation.IntegrationEvents.Customer;

// Flow: Customer.PartyContactPointsChangedIntegrationEvent → Sales.UpdatePartyCacheContactPointsCommand
internal sealed class PartyContactPointsChangedIntegrationEventHandler(
    ISender sender,
    ILogger<PartyContactPointsChangedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<PartyContactPointsChangedIntegrationEvent>
{
    public async Task HandleAsync(
        PartyContactPointsChangedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing PartyContactPointsChanged: PartyId={PartyId}",
            integrationEvent.PartyId);

        var email = integrationEvent.ContactPoints
            .FirstOrDefault(cp => cp.Type == "Email")?.Value;
        var phone = integrationEvent.ContactPoints
            .FirstOrDefault(cp => cp.Type == "Phone")?.Value;

        await sender.Send(
            new UpdatePartyCacheContactPointsCommand(
                integrationEvent.PartyId,
                email,
                phone),
            cancellationToken);
    }

    public Task HandleAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
        => HandleAsync((PartyContactPointsChangedIntegrationEvent)integrationEvent, cancellationToken);
}
