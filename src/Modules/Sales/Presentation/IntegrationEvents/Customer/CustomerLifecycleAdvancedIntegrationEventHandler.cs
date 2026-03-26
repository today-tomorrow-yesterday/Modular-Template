using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Customer.IntegrationEvents;
using Modules.Sales.Application.CustomersCache.UpdateCustomerCacheLifecycle;
using Modules.Sales.Domain.CustomersCache;
using Rtl.Core.Application.EventBus;

namespace Modules.Sales.Presentation.IntegrationEvents.Customer;

// Flow: Customer.CustomerLifecycleAdvancedIntegrationEvent → Sales.UpdateCustomerCacheLifecycleCommand
internal sealed class CustomerLifecycleAdvancedIntegrationEventHandler(
    ISender sender,
    ILogger<CustomerLifecycleAdvancedIntegrationEventHandler> logger)
    : IntegrationEventHandler<CustomerLifecycleAdvancedIntegrationEvent>
{
    public override async Task HandleAsync(
        CustomerLifecycleAdvancedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing CustomerLifecycleAdvanced: PublicCustomerId={PublicCustomerId}, NewStage={NewStage}",
            integrationEvent.PublicCustomerId,
            integrationEvent.NewLifecycleStage);

        await sender.Send(
            new UpdateCustomerCacheLifecycleCommand(
                integrationEvent.PublicCustomerId,
                Enum.Parse<LifecycleStage>(integrationEvent.NewLifecycleStage)),
            cancellationToken);
    }
}
