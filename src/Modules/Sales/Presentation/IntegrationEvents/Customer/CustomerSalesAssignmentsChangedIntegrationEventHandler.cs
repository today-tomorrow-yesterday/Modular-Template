using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Customer.IntegrationEvents;
using Modules.Sales.Application.CustomersCache.UpdateCustomerCacheSalesAssignments;
using Rtl.Core.Application.EventBus;

namespace Modules.Sales.Presentation.IntegrationEvents.Customer;

// Flow: Customer.CustomerSalesAssignmentsChangedIntegrationEvent → Sales.UpdateCustomerCacheSalesAssignmentsCommand
internal sealed class CustomerSalesAssignmentsChangedIntegrationEventHandler(
    ISender sender,
    ILogger<CustomerSalesAssignmentsChangedIntegrationEventHandler> logger)
    : IntegrationEventHandler<CustomerSalesAssignmentsChangedIntegrationEvent>
{
    public override async Task HandleAsync(
        CustomerSalesAssignmentsChangedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing CustomerSalesAssignmentsChanged: CustomerId={CustomerId}",
            integrationEvent.CustomerId);

        var primary = integrationEvent.SalesAssignments
            .FirstOrDefault(sa => sa.Role == "Primary");
        var secondary = integrationEvent.SalesAssignments
            .FirstOrDefault(sa => sa.Role == "Supporting");

        await sender.Send(
            new UpdateCustomerCacheSalesAssignmentsCommand(
                integrationEvent.CustomerId,
                primary?.FederatedId,
                primary?.FirstName,
                primary?.LastName,
                secondary?.FederatedId,
                secondary?.FirstName,
                secondary?.LastName),
            cancellationToken);
    }
}
