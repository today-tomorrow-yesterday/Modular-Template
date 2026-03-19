using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Customer.IntegrationEvents;
using Modules.Sales.Application.CustomersCache.UpsertCustomerCache;
using Modules.Sales.Domain.CustomersCache;
using Rtl.Core.Application.EventBus;

namespace Modules.Sales.Presentation.IntegrationEvents.Customer;

// Flow: Customer.PartyCreatedIntegrationEvent → Sales.UpsertCustomerCacheCommand
internal sealed class CustomerCreatedIntegrationEventHandler(
    ISender sender,
    ILogger<CustomerCreatedIntegrationEventHandler> logger)
    : IntegrationEventHandler<PartyCreatedIntegrationEvent>
{
    public override async Task HandleAsync(
        PartyCreatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing PartyCreated: PartyId={PartyId}",
            integrationEvent.PartyId);

        var displayName = $"{integrationEvent.PersonData?.FirstName} {integrationEvent.PersonData?.LastName}".Trim();

        var salesforceAccountId = integrationEvent.Identifiers
            .FirstOrDefault(i => i.Type == "SalesforceAccountId")?.Value;

        var email = integrationEvent.ContactPoints
            .FirstOrDefault(cp => cp.Type == "Email")?.Value;
        var phone = integrationEvent.ContactPoints
            .FirstOrDefault(cp => cp.Type == "Phone")?.Value;

        string? primaryFederatedId = null, primaryFirstName = null, primaryLastName = null;
        string? secondaryFederatedId = null, secondaryFirstName = null, secondaryLastName = null;

        if (integrationEvent.PersonData is not null)
        {
            var primarySp = integrationEvent.PersonData.SalesAssignments
                .FirstOrDefault(sa => sa.Role == "Primary");
            var secondarySp = integrationEvent.PersonData.SalesAssignments
                .FirstOrDefault(sa => sa.Role == "Supporting");

            primaryFederatedId = primarySp?.FederatedId;
            primaryFirstName = primarySp?.FirstName;
            primaryLastName = primarySp?.LastName;
            secondaryFederatedId = secondarySp?.FederatedId;
            secondaryFirstName = secondarySp?.FirstName;
            secondaryLastName = secondarySp?.LastName;
        }

        var customerCache = new CustomerCache
        {
            RefPublicId = integrationEvent.PartyId,
            LifecycleStage = Enum.Parse<LifecycleStage>(integrationEvent.LifecycleStage),
            HomeCenterNumber = integrationEvent.HomeCenterNumber,
            DisplayName = displayName,
            SalesforceAccountId = salesforceAccountId,
            FirstName = integrationEvent.PersonData?.FirstName ?? string.Empty,
            MiddleName = integrationEvent.PersonData?.MiddleName,
            LastName = integrationEvent.PersonData?.LastName ?? string.Empty,
            Email = email,
            Phone = phone,
            CoBuyerFirstName = integrationEvent.PersonData?.CoBuyerFirstName,
            CoBuyerLastName = integrationEvent.PersonData?.CoBuyerLastName,
            PrimarySalesPersonFederatedId = primaryFederatedId,
            PrimarySalesPersonFirstName = primaryFirstName,
            PrimarySalesPersonLastName = primaryLastName,
            SecondarySalesPersonFederatedId = secondaryFederatedId,
            SecondarySalesPersonFirstName = secondaryFirstName,
            SecondarySalesPersonLastName = secondaryLastName
        };

        await sender.Send(
            new UpsertCustomerCacheCommand(customerCache),
            cancellationToken);
    }
}
