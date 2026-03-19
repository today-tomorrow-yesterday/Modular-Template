using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Customer.IntegrationEvents;
using Modules.Sales.Application.CustomersCache.UpdateCustomerCacheHomeCenter;
using Rtl.Core.Application.EventBus;

namespace Modules.Sales.Presentation.IntegrationEvents.Customer;

// Flow: Customer.PartyHomeCenterChangedIntegrationEvent → Sales.UpdateCustomerCacheHomeCenterCommand
internal sealed class CustomerHomeCenterChangedIntegrationEventHandler(
    ISender sender,
    ILogger<CustomerHomeCenterChangedIntegrationEventHandler> logger)
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
            new UpdateCustomerCacheHomeCenterCommand(
                integrationEvent.PartyId,
                integrationEvent.NewHomeCenterNumber),
            cancellationToken);
    }
}
