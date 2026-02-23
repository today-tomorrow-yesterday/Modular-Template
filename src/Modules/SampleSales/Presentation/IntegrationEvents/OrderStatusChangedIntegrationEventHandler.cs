using Microsoft.Extensions.Logging;
using Modules.SampleOrders.IntegrationEvents;
using Modules.SampleSales.Domain.OrdersCache;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Domain;

namespace Modules.SampleSales.Presentation.IntegrationEvents;

/// <summary>
/// Handles OrderStatusChangedIntegrationEvent from the Orders module.
/// Updates the status in the local OrderCache.
/// </summary>
internal sealed class OrderStatusChangedIntegrationEventHandler(
    ICacheWriteScope cacheWriteScope,
    IOrderCacheRepository orderCacheRepository,
    IOrderCacheWriter orderCacheWriter,
    IDateTimeProvider dateTimeProvider,
    ILogger<OrderStatusChangedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<OrderStatusChangedIntegrationEvent>
{
    public async Task HandleAsync(
        OrderStatusChangedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        using var _ = cacheWriteScope.AllowWrites();

        logger.LogInformation(
            "Processing OrderStatusChanged integration event: OrderId={OrderId}, NewStatus={NewStatus}",
            integrationEvent.OrderId,
            integrationEvent.NewStatus);

        var existingCache = await orderCacheRepository.GetByIdAsync(
            integrationEvent.OrderId,
            cancellationToken);

        if (existingCache is null)
        {
            logger.LogWarning(
                "OrderCache not found for OrderId={OrderId}. Status update skipped.",
                integrationEvent.OrderId);
            return;
        }

        existingCache.Status = integrationEvent.NewStatus;
        existingCache.LastSyncedAtUtc = dateTimeProvider.UtcNow;

        await orderCacheWriter.UpsertAsync(existingCache, cancellationToken);
    }

    public Task HandleAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        return HandleAsync((OrderStatusChangedIntegrationEvent)integrationEvent, cancellationToken);
    }
}
