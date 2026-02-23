using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Customer.IntegrationEvents;
using Modules.Sales.Application.PartiesCache.UpdatePartyCacheCoBuyer;
using Rtl.Core.Application.EventBus;

namespace Modules.Sales.Presentation.IntegrationEvents.Customer;

// Flow: Customer.PartyCoBuyerChangedIntegrationEvent → Sales.UpdatePartyCacheCoBuyerCommand
internal sealed class PartyCoBuyerChangedIntegrationEventHandler(
    ISender sender,
    ILogger<PartyCoBuyerChangedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<PartyCoBuyerChangedIntegrationEvent>
{
    public async Task HandleAsync(
        PartyCoBuyerChangedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing PartyCoBuyerChanged: PartyId={PartyId}, CoBuyerPartyId={CoBuyerPartyId}",
            integrationEvent.PartyId,
            integrationEvent.CoBuyerPartyId);

        await sender.Send(
            new UpdatePartyCacheCoBuyerCommand(
                integrationEvent.PartyId,
                integrationEvent.CoBuyerFirstName,
                integrationEvent.CoBuyerLastName),
            cancellationToken);
    }

    public Task HandleAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
        => HandleAsync((PartyCoBuyerChangedIntegrationEvent)integrationEvent, cancellationToken);
}
