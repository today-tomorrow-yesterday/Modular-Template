using Microsoft.Extensions.Logging;
using Modules.Customer.IntegrationEvents;
using Modules.Funding.Domain.CustomersCache;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Domain;

namespace Modules.Funding.Presentation.IntegrationEvents;

// Flow: Customer.PartyCreatedIntegrationEvent → Funding.UpsertCustomerCache
internal sealed class PartyCreatedIntegrationEventHandler(
    ICacheWriteScope cacheWriteScope,
    ICustomerCacheWriter customerCacheWriter,
    IDateTimeProvider dateTimeProvider,
    ILogger<PartyCreatedIntegrationEventHandler> logger)
    : IntegrationEventHandler<PartyCreatedIntegrationEvent>
{
    public override async Task HandleAsync(
        PartyCreatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        using var _ = cacheWriteScope.AllowWrites();

        var loanId = integrationEvent.Identifiers
            .FirstOrDefault(i => i.Type == "LoanId")?.Value;

        logger.LogInformation(
            "Processing PartyCreated: PartyId={PartyId}",
            integrationEvent.PartyId);

        var customerCache = new CustomerCache
        {
            Id = integrationEvent.PartyId,
            LoanId = loanId,
            FirstName = integrationEvent.PersonData?.FirstName ?? string.Empty,
            LastName = integrationEvent.PersonData?.LastName ?? string.Empty,
            HomeCenterNumber = integrationEvent.HomeCenterNumber,
            LastSyncedAtUtc = dateTimeProvider.UtcNow
        };

        await customerCacheWriter.UpsertAsync(customerCache, cancellationToken);
    }
}
