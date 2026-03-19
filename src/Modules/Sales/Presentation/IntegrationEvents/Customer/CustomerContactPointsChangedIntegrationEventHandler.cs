using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Customer.IntegrationEvents;
using Modules.Sales.Application.CustomersCache.UpdateCustomerCacheContactPoints;
using Rtl.Core.Application.EventBus;

namespace Modules.Sales.Presentation.IntegrationEvents.Customer;

// Flow: Customer.PartyContactPointsChangedIntegrationEvent → Sales.UpdateCustomerCacheContactPointsCommand
internal sealed class CustomerContactPointsChangedIntegrationEventHandler(
    ISender sender,
    ILogger<CustomerContactPointsChangedIntegrationEventHandler> logger)
    : IntegrationEventHandler<PartyContactPointsChangedIntegrationEvent>
{
    public override async Task HandleAsync(
        PartyContactPointsChangedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing PartyContactPointsChanged: PartyId={PartyId}",
            integrationEvent.PartyId);

        var email = integrationEvent.ContactPoints
            .FirstOrDefault(cp => cp.Type == "Email")?.Value;
        var phone = integrationEvent.ContactPoints
            .FirstOrDefault(cp => cp.Type == "Phone")?.Value;

        await sender.Send(
            new UpdateCustomerCacheContactPointsCommand(
                integrationEvent.PartyId,
                email,
                phone),
            cancellationToken);
    }
}
