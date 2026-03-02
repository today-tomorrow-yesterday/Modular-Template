using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Customer.IntegrationEvents;
using Modules.Sales.Application.PartiesCache.UpdatePartyCacheHomeCenter;
using Rtl.Core.Application.EventBus;

namespace Modules.Sales.Presentation.IntegrationEvents.Customer;

// Flow: Customer.PartyHomeCenterChangedIntegrationEvent → Sales.UpdatePartyCacheHomeCenterCommand
internal sealed class PartyHomeCenterChangedIntegrationEventHandler(
    ISender sender,
    ILogger<PartyHomeCenterChangedIntegrationEventHandler> logger)
    : IntegrationEventHandler<PartyHomeCenterChangedIntegrationEvent>
{
    public override async Task HandleAsync(
        PartyHomeCenterChangedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing PartyHomeCenterChanged: PartyId={PartyId}, NewHC={NewHomeCenterNumber}",
            integrationEvent.PartyId,
            integrationEvent.NewHomeCenterNumber);

        await sender.Send(
            new UpdatePartyCacheHomeCenterCommand(
                integrationEvent.PartyId,
                integrationEvent.NewHomeCenterNumber),
            cancellationToken);
    }
}
