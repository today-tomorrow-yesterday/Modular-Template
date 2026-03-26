using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Customer.IntegrationEvents;
using Modules.Sales.Application.CustomersCache.UpdateCustomerCacheMailingAddress;
using Rtl.Core.Application.EventBus;

namespace Modules.Sales.Presentation.IntegrationEvents.Customer;

// Flow: Customer.CustomerMailingAddressChangedIntegrationEvent → Sales.UpdateCustomerCacheMailingAddressCommand
internal sealed class CustomerMailingAddressChangedIntegrationEventHandler(
    ISender sender,
    ILogger<CustomerMailingAddressChangedIntegrationEventHandler> logger)
    : IntegrationEventHandler<CustomerMailingAddressChangedIntegrationEvent>
{
    public override async Task HandleAsync(
        CustomerMailingAddressChangedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing CustomerMailingAddressChanged: PublicCustomerId={PublicCustomerId}",
            integrationEvent.PublicCustomerId);

        await sender.Send(
            new UpdateCustomerCacheMailingAddressCommand(
                integrationEvent.PublicCustomerId),
            cancellationToken);
    }
}
