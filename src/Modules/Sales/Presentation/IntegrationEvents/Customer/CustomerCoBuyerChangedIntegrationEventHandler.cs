using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Customer.IntegrationEvents;
using Modules.Sales.Application.CustomersCache.UpdateCustomerCacheCoBuyer;
using Rtl.Core.Application.EventBus;

namespace Modules.Sales.Presentation.IntegrationEvents.Customer;

// Flow: Customer.PartyCoBuyerChangedIntegrationEvent → Sales.UpdateCustomerCacheCoBuyerCommand
internal sealed class CustomerCoBuyerChangedIntegrationEventHandler(
    ISender sender,
    ILogger<CustomerCoBuyerChangedIntegrationEventHandler> logger)
    : IntegrationEventHandler<PartyCoBuyerChangedIntegrationEvent>
{
    public override async Task HandleAsync(
        PartyCoBuyerChangedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing PartyCoBuyerChanged: PartyId={PartyId}, CoBuyerPublicId={CoBuyerPublicId}",
            integrationEvent.PartyId,
            integrationEvent.CoBuyerPublicId);

        await sender.Send(
            new UpdateCustomerCacheCoBuyerCommand(
                integrationEvent.PartyId,
                integrationEvent.CoBuyerFirstName,
                integrationEvent.CoBuyerLastName),
            cancellationToken);
    }
}
