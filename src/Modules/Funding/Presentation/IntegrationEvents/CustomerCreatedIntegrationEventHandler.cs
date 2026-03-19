using Microsoft.Extensions.Logging;
using Modules.Customer.IntegrationEvents;
using Modules.Funding.Domain.CustomersCache;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Domain;

namespace Modules.Funding.Presentation.IntegrationEvents;

// Flow: Customer.CustomerCreatedIntegrationEvent → Funding.UpsertCustomerCache
internal sealed class CustomerCreatedIntegrationEventHandler(
    ICacheWriteScope cacheWriteScope,
    ICustomerCacheWriter customerCacheWriter,
    IDateTimeProvider dateTimeProvider,
    ILogger<CustomerCreatedIntegrationEventHandler> logger)
    : IntegrationEventHandler<CustomerCreatedIntegrationEvent>
{
    public override async Task HandleAsync(
        CustomerCreatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        using var _ = cacheWriteScope.AllowWrites();

        var loanId = integrationEvent.Identifiers
            .FirstOrDefault(i => i.Type == "LoanId")?.Value;

        logger.LogInformation(
            "Processing CustomerCreated: CustomerId={CustomerId}",
            integrationEvent.CustomerId);

        var customerCache = new CustomerCache
        {
            RefPublicId = integrationEvent.CustomerId,
            LoanId = loanId,
            FirstName = integrationEvent.FirstName ?? string.Empty,
            LastName = integrationEvent.LastName ?? string.Empty,
            HomeCenterNumber = integrationEvent.HomeCenterNumber,
            LastSyncedAtUtc = dateTimeProvider.UtcNow
        };

        await customerCacheWriter.UpsertAsync(customerCache, cancellationToken);
    }
}
