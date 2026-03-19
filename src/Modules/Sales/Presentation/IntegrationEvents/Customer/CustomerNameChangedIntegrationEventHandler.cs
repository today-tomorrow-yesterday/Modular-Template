using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Customer.IntegrationEvents;
using Modules.Sales.Application.CustomersCache.UpdateCustomerCacheName;
using Rtl.Core.Application.EventBus;

namespace Modules.Sales.Presentation.IntegrationEvents.Customer;

// Flow: Customer.CustomerNameChangedIntegrationEvent → Sales.UpdateCustomerCacheNameCommand
internal sealed class CustomerNameChangedIntegrationEventHandler(
    ISender sender,
    ILogger<CustomerNameChangedIntegrationEventHandler> logger)
    : IntegrationEventHandler<CustomerNameChangedIntegrationEvent>
{
    public override async Task HandleAsync(
        CustomerNameChangedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing CustomerNameChanged: CustomerId={CustomerId}",
            integrationEvent.CustomerId);

        var displayName = $"{integrationEvent.FirstName} {integrationEvent.LastName}".Trim();

        await sender.Send(
            new UpdateCustomerCacheNameCommand(
                integrationEvent.CustomerId,
                displayName,
                integrationEvent.FirstName,
                integrationEvent.MiddleName,
                integrationEvent.LastName),
            cancellationToken);
    }
}
