using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Customer.IntegrationEvents;
using Modules.Sales.Application.CustomersCache.UpdateCustomerCacheHomeCenter;
using Rtl.Core.Application.EventBus;

namespace Modules.Sales.Presentation.IntegrationEvents.Customer;

// Flow: Customer.CustomerHomeCenterChangedIntegrationEvent → Sales.UpdateCustomerCacheHomeCenterCommand
internal sealed class CustomerHomeCenterChangedIntegrationEventHandler(
    ISender sender,
    ILogger<CustomerHomeCenterChangedIntegrationEventHandler> logger)
    : IntegrationEventHandler<CustomerHomeCenterChangedIntegrationEvent>
{
    public override async Task HandleAsync(
        CustomerHomeCenterChangedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing CustomerHomeCenterChanged: CustomerId={CustomerId}, NewHC={NewHomeCenterNumber}",
            integrationEvent.CustomerId,
            integrationEvent.NewHomeCenterNumber);

        await sender.Send(
            new UpdateCustomerCacheHomeCenterCommand(
                integrationEvent.CustomerId,
                integrationEvent.NewHomeCenterNumber),
            cancellationToken);
    }
}
