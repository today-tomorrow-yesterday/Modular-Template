using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Customer.IntegrationEvents;
using Modules.Sales.Application.PartiesCache.UpdatePartyCacheName;
using Modules.Sales.Domain.PartiesCache;
using Rtl.Core.Application.EventBus;

namespace Modules.Sales.Presentation.IntegrationEvents.Customer;

// Flow: Customer.PartyNameChangedIntegrationEvent → Sales.UpdatePartyCacheNameCommand
internal sealed class PartyNameChangedIntegrationEventHandler(
    ISender sender,
    ILogger<PartyNameChangedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<PartyNameChangedIntegrationEvent>
{
    public async Task HandleAsync(
        PartyNameChangedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing PartyNameChanged: PartyId={PartyId}, PartyType={PartyType}",
            integrationEvent.PartyId,
            integrationEvent.PartyType);

        var displayName = integrationEvent.PartyType == "Person"
            ? $"{integrationEvent.FirstName} {integrationEvent.LastName}".Trim()
            : integrationEvent.OrganizationName ?? string.Empty;

        await sender.Send(
            new UpdatePartyCacheNameCommand(
                integrationEvent.PartyId,
                Enum.Parse<PartyType>(integrationEvent.PartyType),
                displayName,
                integrationEvent.FirstName,
                integrationEvent.MiddleName,
                integrationEvent.LastName,
                integrationEvent.OrganizationName),
            cancellationToken);
    }

    public Task HandleAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
        => HandleAsync((PartyNameChangedIntegrationEvent)integrationEvent, cancellationToken);
}
