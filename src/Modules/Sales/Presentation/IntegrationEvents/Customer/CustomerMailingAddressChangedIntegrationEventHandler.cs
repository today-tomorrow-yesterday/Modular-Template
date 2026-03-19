using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Customer.IntegrationEvents;
using Modules.Sales.Application.CustomersCache.UpdateCustomerCacheMailingAddress;
using Rtl.Core.Application.EventBus;

namespace Modules.Sales.Presentation.IntegrationEvents.Customer;

// Flow: Customer.PartyMailingAddressChangedIntegrationEvent → Sales.UpdateCustomerCacheMailingAddressCommand
internal sealed class CustomerMailingAddressChangedIntegrationEventHandler(
    ISender sender,
    ILogger<CustomerMailingAddressChangedIntegrationEventHandler> logger)
    : IntegrationEventHandler<PartyMailingAddressChangedIntegrationEvent>
{
    public override async Task HandleAsync(
        PartyMailingAddressChangedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing PartyMailingAddressChanged: PartyId={PartyId}",
            integrationEvent.PartyId);

        await sender.Send(
            new UpdateCustomerCacheMailingAddressCommand(
                integrationEvent.PartyId),
            cancellationToken);
    }
}
