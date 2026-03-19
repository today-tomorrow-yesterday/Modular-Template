using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Customer.IntegrationEvents;
using Modules.Sales.Application.CustomersCache.UpdateCustomerCacheLifecycle;
using Modules.Sales.Domain.CustomersCache;
using Rtl.Core.Application.EventBus;

namespace Modules.Sales.Presentation.IntegrationEvents.Customer;

// Flow: Customer.PartyLifecycleAdvancedIntegrationEvent → Sales.UpdateCustomerCacheLifecycleCommand
internal sealed class CustomerLifecycleAdvancedIntegrationEventHandler(
    ISender sender,
    ILogger<CustomerLifecycleAdvancedIntegrationEventHandler> logger)
    : IntegrationEventHandler<PartyLifecycleAdvancedIntegrationEvent>
{
    public override async Task HandleAsync(
        PartyLifecycleAdvancedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing PartyLifecycleAdvanced: PartyId={PartyId}, NewStage={NewStage}",
            integrationEvent.PartyId,
            integrationEvent.NewLifecycleStage);

        await sender.Send(
            new UpdateCustomerCacheLifecycleCommand(
                integrationEvent.PartyId,
                Enum.Parse<LifecycleStage>(integrationEvent.NewLifecycleStage)),
            cancellationToken);
    }
}
