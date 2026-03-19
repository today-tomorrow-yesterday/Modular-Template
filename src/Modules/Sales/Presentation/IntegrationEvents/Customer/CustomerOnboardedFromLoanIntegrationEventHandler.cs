using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Customer.IntegrationEvents;
using Modules.Sales.Application.CustomersCache.UpsertCustomerCache;
using Modules.Sales.Domain.CustomersCache;
using Rtl.Core.Application.EventBus;

namespace Modules.Sales.Presentation.IntegrationEvents.Customer;

// Flow: Customer.PartyOnboardedFromLoanIntegrationEvent → Sales.UpsertCustomerCacheCommand
internal sealed class CustomerOnboardedFromLoanIntegrationEventHandler(
    ISender sender,
    ILogger<CustomerOnboardedFromLoanIntegrationEventHandler> logger)
    : IntegrationEventHandler<PartyOnboardedFromLoanIntegrationEvent>
{
    public override async Task HandleAsync(
        PartyOnboardedFromLoanIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing PartyOnboardedFromLoan: PartyId={PartyId}",
            integrationEvent.PartyId);

        var displayName = $"{integrationEvent.FirstName} {integrationEvent.LastName}".Trim();

        var email = integrationEvent.ContactPoints
            .FirstOrDefault(cp => cp.Type == "Email")?.Value;
        var phone = integrationEvent.ContactPoints
            .FirstOrDefault(cp => cp.Type == "Phone")?.Value;

        var customerCache = new CustomerCache
        {
            RefPublicId = integrationEvent.PartyId,
            LifecycleStage = LifecycleStage.Customer,
            HomeCenterNumber = integrationEvent.HomeCenterNumber,
            DisplayName = displayName,
            FirstName = integrationEvent.FirstName ?? string.Empty,
            MiddleName = integrationEvent.MiddleName,
            LastName = integrationEvent.LastName ?? string.Empty,
            Email = email,
            Phone = phone
        };

        await sender.Send(
            new UpsertCustomerCacheCommand(customerCache),
            cancellationToken);
    }
}
