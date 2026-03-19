using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Customer.IntegrationEvents;
using Modules.Sales.Application.CustomersCache.UpdateCustomerCacheName;
using Rtl.Core.Application.EventBus;

namespace Modules.Sales.Presentation.IntegrationEvents.Customer;

// Flow: Customer.PartyNameChangedIntegrationEvent → Sales.UpdateCustomerCacheNameCommand
internal sealed class CustomerNameChangedIntegrationEventHandler(
    ISender sender,
    ILogger<CustomerNameChangedIntegrationEventHandler> logger)
    : IntegrationEventHandler<PartyNameChangedIntegrationEvent>
{
    public override async Task HandleAsync(
        PartyNameChangedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing PartyNameChanged: PartyId={PartyId}",
            integrationEvent.PartyId);

        var displayName = $"{integrationEvent.FirstName} {integrationEvent.LastName}".Trim();

        await sender.Send(
            new UpdateCustomerCacheNameCommand(
                integrationEvent.PartyId,
                displayName,
                integrationEvent.FirstName,
                integrationEvent.MiddleName,
                integrationEvent.LastName),
            cancellationToken);
    }
}
