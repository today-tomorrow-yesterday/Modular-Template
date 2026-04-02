using Microsoft.Extensions.Logging;
using Modules.SampleOrders.IntegrationEvents;
using Modules.SampleSales.Domain.OrdersCache;
using ModularTemplate.Application.Caching;
using ModularTemplate.Application.EventBus;
using ModularTemplate.Domain;

namespace Modules.SampleSales.Presentation.IntegrationEvents;

internal sealed class OrderStatusChangedIntegrationEventHandler(
    ICacheWriteScope cacheWriteScope,
    IOrderCacheRepository orderCacheRepository,
    IOrderCacheWriter orderCacheWriter,
    IDateTimeProvider dateTimeProvider,
    ILogger<OrderStatusChangedIntegrationEventHandler> logger)
    : IntegrationEventHandler<OrderStatusChangedIntegrationEvent>
{
    public override async Task HandleAsync(
        OrderStatusChangedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        using var _ = cacheWriteScope.AllowWrites();

        logger.LogInformation(
            "Processing OrderStatusChanged: PublicOrderId={PublicOrderId}, NewStatus={NewStatus}",
            integrationEvent.PublicOrderId,
            integrationEvent.NewStatus);

        var existing = await orderCacheRepository.GetByRefPublicIdAsync(
            integrationEvent.PublicOrderId,
            cancellationToken);

        if (existing is null)
        {
            logger.LogWarning(
                "OrderCache not found for PublicOrderId={PublicOrderId}. Skipping status update — the OrderCreated event may not have been processed yet.",
                integrationEvent.PublicOrderId);
            return;
        }

        existing.Status = integrationEvent.NewStatus;
        existing.LastSyncedAtUtc = dateTimeProvider.UtcNow;

        await orderCacheWriter.UpsertAsync(existing, cancellationToken);
    }
}
