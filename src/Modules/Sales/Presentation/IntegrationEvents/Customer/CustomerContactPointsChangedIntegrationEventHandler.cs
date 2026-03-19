using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Customer.IntegrationEvents;
using Modules.Sales.Application.CustomersCache.UpdateCustomerCacheContactPoints;
using Rtl.Core.Application.EventBus;

namespace Modules.Sales.Presentation.IntegrationEvents.Customer;

// Flow: Customer.CustomerContactPointsChangedIntegrationEvent → Sales.UpdateCustomerCacheContactPointsCommand
internal sealed class CustomerContactPointsChangedIntegrationEventHandler(
    ISender sender,
    ILogger<CustomerContactPointsChangedIntegrationEventHandler> logger)
    : IntegrationEventHandler<CustomerContactPointsChangedIntegrationEvent>
{
    public override async Task HandleAsync(
        CustomerContactPointsChangedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing CustomerContactPointsChanged: CustomerId={CustomerId}",
            integrationEvent.CustomerId);

        var email = integrationEvent.ContactPoints
            .FirstOrDefault(cp => cp.Type == "Email")?.Value;
        var phone = integrationEvent.ContactPoints
            .FirstOrDefault(cp => cp.Type == "Phone")?.Value;

        await sender.Send(
            new UpdateCustomerCacheContactPointsCommand(
                integrationEvent.CustomerId,
                email,
                phone),
            cancellationToken);
    }
}
