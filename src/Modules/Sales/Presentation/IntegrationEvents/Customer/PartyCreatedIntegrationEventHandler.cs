using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Customer.IntegrationEvents;
using Modules.Sales.Application.PartiesCache.UpsertPartyCache;
using Modules.Sales.Domain.PartiesCache;
using Rtl.Core.Application.EventBus;

namespace Modules.Sales.Presentation.IntegrationEvents.Customer;

// Flow: Customer.PartyCreatedIntegrationEvent → Sales.UpsertPartyCacheCommand
internal sealed class PartyCreatedIntegrationEventHandler(
    ISender sender,
    ILogger<PartyCreatedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<PartyCreatedIntegrationEvent>
{
    public async Task HandleAsync(
        PartyCreatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing PartyCreated: PartyId={PartyId}, PartyType={PartyType}",
            integrationEvent.PartyId,
            integrationEvent.PartyType);

        var displayName = integrationEvent.PartyType == "Person"
            ? $"{integrationEvent.PersonData?.FirstName} {integrationEvent.PersonData?.LastName}".Trim()
            : integrationEvent.OrganizationData?.OrganizationName ?? string.Empty;

        var salesforceAccountId = integrationEvent.Identifiers
            .FirstOrDefault(i => i.Type == "SalesforceAccountId")?.Value;

        var partyCache = new PartyCache
        {
            RefPartyId = integrationEvent.PartyId,
            RefPublicId = integrationEvent.PublicId,
            PartyType = Enum.Parse<PartyType>(integrationEvent.PartyType),
            LifecycleStage = Enum.Parse<LifecycleStage>(integrationEvent.LifecycleStage),
            HomeCenterNumber = integrationEvent.HomeCenterNumber,
            DisplayName = displayName,
            SalesforceAccountId = salesforceAccountId
        };

        PartyPersonCache? personCache = null;
        PartyOrganizationCache? organizationCache = null;

        if (integrationEvent.PartyType == "Person" && integrationEvent.PersonData is not null)
        {
            var primarySp = integrationEvent.PersonData.SalesAssignments
                .FirstOrDefault(sa => sa.Role == "Primary");
            var secondarySp = integrationEvent.PersonData.SalesAssignments
                .FirstOrDefault(sa => sa.Role == "Supporting");

            var email = integrationEvent.ContactPoints
                .FirstOrDefault(cp => cp.Type == "Email")?.Value;
            var phone = integrationEvent.ContactPoints
                .FirstOrDefault(cp => cp.Type == "Phone")?.Value;

            personCache = new PartyPersonCache
            {
                FirstName = integrationEvent.PersonData.FirstName ?? string.Empty,
                MiddleName = integrationEvent.PersonData.MiddleName,
                LastName = integrationEvent.PersonData.LastName ?? string.Empty,
                Email = email,
                Phone = phone,
                CoBuyerFirstName = integrationEvent.PersonData.CoBuyerFirstName,
                CoBuyerLastName = integrationEvent.PersonData.CoBuyerLastName,
                PrimarySalesPersonFederatedId = primarySp?.FederatedId,
                PrimarySalesPersonFirstName = primarySp?.FirstName,
                PrimarySalesPersonLastName = primarySp?.LastName,
                SecondarySalesPersonFederatedId = secondarySp?.FederatedId,
                SecondarySalesPersonFirstName = secondarySp?.FirstName,
                SecondarySalesPersonLastName = secondarySp?.LastName
            };
        }
        else if (integrationEvent.PartyType == "Organization" && integrationEvent.OrganizationData is not null)
        {
            organizationCache = new PartyOrganizationCache
            {
                OrganizationName = integrationEvent.OrganizationData.OrganizationName ?? string.Empty
            };
        }

        await sender.Send(
            new UpsertPartyCacheCommand(partyCache, personCache, organizationCache),
            cancellationToken);
    }

    public Task HandleAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
        => HandleAsync((PartyCreatedIntegrationEvent)integrationEvent, cancellationToken);
}
