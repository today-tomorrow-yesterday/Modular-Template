using Microsoft.Extensions.Logging;
using Modules.SampleOrders.IntegrationEvents;
using Modules.SampleSales.Domain.OrdersCache;
using ModularTemplate.Application.Caching;
using ModularTemplate.Application.EventBus;
using ModularTemplate.Domain;

namespace Modules.SampleSales.Presentation.IntegrationEvents;

internal sealed class OrderStatusChangedIntegrationEventHandler(
    ICacheWriteScope cacheWriteScope,
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

        // TODO: Implement upsert-by-RefPublicId lookup pattern
        // For now, create a new cache entry (proper implementation would find existing by RefPublicId)
        var orderCache = new OrderCache
        {
            RefPublicId = integrationEvent.PublicOrderId,
            Status = integrationEvent.NewStatus,
            LastSyncedAtUtc = dateTimeProvider.UtcNow
        };

        await orderCacheWriter.UpsertAsync(orderCache, cancellationToken);
    }
}
