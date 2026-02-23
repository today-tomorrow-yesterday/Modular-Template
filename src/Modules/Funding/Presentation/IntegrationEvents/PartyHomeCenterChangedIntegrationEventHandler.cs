using Microsoft.Extensions.Logging;
using Modules.Customer.IntegrationEvents;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Domain;

namespace Modules.Funding.Presentation.IntegrationEvents;

// Flow: Customer.PartyHomeCenterChangedIntegrationEvent → Funding.UpdateCustomerCacheHomeCenter
internal sealed class PartyHomeCenterChangedIntegrationEventHandler(
    ICacheWriteScope cacheWriteScope,
    ICustomerCacheWriter customerCacheWriter,
    IDateTimeProvider dateTimeProvider,
    ILogger<PartyHomeCenterChangedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<PartyHomeCenterChangedIntegrationEvent>
{
    public async Task HandleAsync(
        PartyHomeCenterChangedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        using var _ = cacheWriteScope.AllowWrites();

        logger.LogInformation(
            "Processing PartyHomeCenterChanged: PartyId={PartyId}, NewHC={NewHomeCenterNumber}",
            integrationEvent.PartyId,
            integrationEvent.NewHomeCenterNumber);

        await customerCacheWriter.UpdateHomeCenterAsync(
            integrationEvent.PartyId,
            integrationEvent.NewHomeCenterNumber,
            dateTimeProvider.UtcNow,
            cancellationToken);
    }

    public Task HandleAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
        => HandleAsync((PartyHomeCenterChangedIntegrationEvent)integrationEvent, cancellationToken);
}
