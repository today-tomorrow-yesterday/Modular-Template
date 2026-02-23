using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Customer.IntegrationEvents;
using Modules.Sales.Application.PartiesCache.UpdatePartyCacheMailingAddress;
using Rtl.Core.Application.EventBus;

namespace Modules.Sales.Presentation.IntegrationEvents.Customer;

// Flow: Customer.PartyMailingAddressChangedIntegrationEvent → Sales.UpdatePartyCacheMailingAddressCommand
internal sealed class PartyMailingAddressChangedIntegrationEventHandler(
    ISender sender,
    ILogger<PartyMailingAddressChangedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<PartyMailingAddressChangedIntegrationEvent>
{
    public async Task HandleAsync(
        PartyMailingAddressChangedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing PartyMailingAddressChanged: PartyId={PartyId}",
            integrationEvent.PartyId);

        await sender.Send(
            new UpdatePartyCacheMailingAddressCommand(
                integrationEvent.PartyId),
            cancellationToken);
    }

    public Task HandleAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
        => HandleAsync((PartyMailingAddressChangedIntegrationEvent)integrationEvent, cancellationToken);
}
