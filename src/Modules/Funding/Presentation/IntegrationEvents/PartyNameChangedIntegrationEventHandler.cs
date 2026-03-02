using Microsoft.Extensions.Logging;
using Modules.Customer.IntegrationEvents;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Domain;

namespace Modules.Funding.Presentation.IntegrationEvents;

// Flow: Customer.PartyNameChangedIntegrationEvent → Funding.UpdateCustomerCacheName
internal sealed class PartyNameChangedIntegrationEventHandler(
    ICacheWriteScope cacheWriteScope,
    ICustomerCacheWriter customerCacheWriter,
    IDateTimeProvider dateTimeProvider,
    ILogger<PartyNameChangedIntegrationEventHandler> logger)
    : IntegrationEventHandler<PartyNameChangedIntegrationEvent>
{
    public override async Task HandleAsync(
        PartyNameChangedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        using var _ = cacheWriteScope.AllowWrites();

        logger.LogInformation(
            "Processing PartyNameChanged: PartyId={PartyId}",
            integrationEvent.PartyId);

        await customerCacheWriter.UpdateNameAsync(
            integrationEvent.PartyId,
            integrationEvent.FirstName ?? string.Empty,
            integrationEvent.LastName ?? string.Empty,
            dateTimeProvider.UtcNow,
            cancellationToken);
    }
}
