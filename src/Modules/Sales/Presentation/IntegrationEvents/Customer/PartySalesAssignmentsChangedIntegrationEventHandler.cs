using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Customer.IntegrationEvents;
using Modules.Sales.Application.PartiesCache.UpdatePartyCacheSalesAssignments;
using Rtl.Core.Application.EventBus;

namespace Modules.Sales.Presentation.IntegrationEvents.Customer;

// Flow: Customer.PartySalesAssignmentsChangedIntegrationEvent → Sales.UpdatePartyCacheSalesAssignmentsCommand
internal sealed class PartySalesAssignmentsChangedIntegrationEventHandler(
    ISender sender,
    ILogger<PartySalesAssignmentsChangedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<PartySalesAssignmentsChangedIntegrationEvent>
{
    public async Task HandleAsync(
        PartySalesAssignmentsChangedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing PartySalesAssignmentsChanged: PartyId={PartyId}",
            integrationEvent.PartyId);

        var primary = integrationEvent.SalesAssignments
            .FirstOrDefault(sa => sa.Role == "Primary");
        var secondary = integrationEvent.SalesAssignments
            .FirstOrDefault(sa => sa.Role == "Supporting");

        await sender.Send(
            new UpdatePartyCacheSalesAssignmentsCommand(
                integrationEvent.PartyId,
                primary?.FederatedId,
                primary?.FirstName,
                primary?.LastName,
                secondary?.FederatedId,
                secondary?.FirstName,
                secondary?.LastName),
            cancellationToken);
    }

    public Task HandleAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
        => HandleAsync((PartySalesAssignmentsChangedIntegrationEvent)integrationEvent, cancellationToken);
}
