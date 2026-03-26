using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Customer.IntegrationEvents;
using Modules.Sales.Application.CustomersCache.UpsertCustomerCache;
using Modules.Sales.Domain.CustomersCache;
using Rtl.Core.Application.EventBus;

namespace Modules.Sales.Presentation.IntegrationEvents.Customer;

// Flow: Customer.CustomerOnboardedFromLoanIntegrationEvent → Sales.UpsertCustomerCacheCommand
internal sealed class CustomerOnboardedFromLoanIntegrationEventHandler(
    ISender sender,
    ILogger<CustomerOnboardedFromLoanIntegrationEventHandler> logger)
    : IntegrationEventHandler<CustomerOnboardedFromLoanIntegrationEvent>
{
    public override async Task HandleAsync(
        CustomerOnboardedFromLoanIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing CustomerOnboardedFromLoan: PublicCustomerId={PublicCustomerId}",
            integrationEvent.PublicCustomerId);

        var displayName = $"{integrationEvent.FirstName} {integrationEvent.LastName}".Trim();

        var email = integrationEvent.ContactPoints
            .FirstOrDefault(cp => cp.Type == "Email")?.Value;
        var phone = integrationEvent.ContactPoints
            .FirstOrDefault(cp => cp.Type == "Phone")?.Value;

        var customerCache = new CustomerCache
        {
            RefPublicId = integrationEvent.PublicCustomerId,
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
