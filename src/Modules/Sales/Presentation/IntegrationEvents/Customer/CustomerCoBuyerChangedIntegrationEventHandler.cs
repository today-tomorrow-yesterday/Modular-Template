using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Customer.IntegrationEvents;
using Modules.Sales.Application.CustomersCache.UpdateCustomerCacheCoBuyer;
using Rtl.Core.Application.EventBus;

namespace Modules.Sales.Presentation.IntegrationEvents.Customer;

// Flow: Customer.CustomerCoBuyerChangedIntegrationEvent → Sales.UpdateCustomerCacheCoBuyerCommand
internal sealed class CustomerCoBuyerChangedIntegrationEventHandler(
    ISender sender,
    ILogger<CustomerCoBuyerChangedIntegrationEventHandler> logger)
    : IntegrationEventHandler<CustomerCoBuyerChangedIntegrationEvent>
{
    public override async Task HandleAsync(
        CustomerCoBuyerChangedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing CustomerCoBuyerChanged: CustomerId={CustomerId}, CoBuyerPublicId={CoBuyerPublicId}",
            integrationEvent.CustomerId,
            integrationEvent.CoBuyerPublicId);

        await sender.Send(
            new UpdateCustomerCacheCoBuyerCommand(
                integrationEvent.CustomerId,
                integrationEvent.CoBuyerFirstName,
                integrationEvent.CoBuyerLastName),
            cancellationToken);
    }
}
