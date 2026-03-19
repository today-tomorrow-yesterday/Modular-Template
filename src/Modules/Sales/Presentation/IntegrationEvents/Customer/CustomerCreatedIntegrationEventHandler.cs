using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Customer.IntegrationEvents;
using Modules.Sales.Application.CustomersCache.UpsertCustomerCache;
using Modules.Sales.Domain.CustomersCache;
using Rtl.Core.Application.EventBus;

namespace Modules.Sales.Presentation.IntegrationEvents.Customer;

// Flow: Customer.CustomerCreatedIntegrationEvent → Sales.UpsertCustomerCacheCommand
internal sealed class CustomerCreatedIntegrationEventHandler(
    ISender sender,
    ILogger<CustomerCreatedIntegrationEventHandler> logger)
    : IntegrationEventHandler<CustomerCreatedIntegrationEvent>
{
    public override async Task HandleAsync(
        CustomerCreatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing CustomerCreated: CustomerId={CustomerId}",
            integrationEvent.CustomerId);

        var displayName = $"{integrationEvent.FirstName} {integrationEvent.LastName}".Trim();

        var salesforceAccountId = integrationEvent.Identifiers
            .FirstOrDefault(i => i.Type == "SalesforceAccountId")?.Value;

        var email = integrationEvent.ContactPoints
            .FirstOrDefault(cp => cp.Type == "Email")?.Value;
        var phone = integrationEvent.ContactPoints
            .FirstOrDefault(cp => cp.Type == "Phone")?.Value;

        var primarySp = integrationEvent.SalesAssignments
            .FirstOrDefault(sa => sa.Role == "Primary");
        var secondarySp = integrationEvent.SalesAssignments
            .FirstOrDefault(sa => sa.Role == "Supporting");

        var customerCache = new CustomerCache
        {
            RefPublicId = integrationEvent.CustomerId,
            LifecycleStage = Enum.Parse<LifecycleStage>(integrationEvent.LifecycleStage),
            HomeCenterNumber = integrationEvent.HomeCenterNumber,
            DisplayName = displayName,
            SalesforceAccountId = salesforceAccountId,
            FirstName = integrationEvent.FirstName ?? string.Empty,
            MiddleName = integrationEvent.MiddleName,
            LastName = integrationEvent.LastName ?? string.Empty,
            Email = email,
            Phone = phone,
            CoBuyerFirstName = integrationEvent.CoBuyerFirstName,
            CoBuyerLastName = integrationEvent.CoBuyerLastName,
            PrimarySalesPersonFederatedId = primarySp?.FederatedId,
            PrimarySalesPersonFirstName = primarySp?.FirstName,
            PrimarySalesPersonLastName = primarySp?.LastName,
            SecondarySalesPersonFederatedId = secondarySp?.FederatedId,
            SecondarySalesPersonFirstName = secondarySp?.FirstName,
            SecondarySalesPersonLastName = secondarySp?.LastName
        };

        await sender.Send(
            new UpsertCustomerCacheCommand(customerCache),
            cancellationToken);
    }
}
