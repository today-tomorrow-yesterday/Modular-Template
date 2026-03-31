using Microsoft.Extensions.Logging;
using Modules.SampleOrders.IntegrationEvents;
using Modules.SampleSales.Domain.OrdersCache;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Domain;

namespace Modules.SampleSales.Presentation.IntegrationEvents;

internal sealed class OrderPlacedIntegrationEventHandler(
    ICacheWriteScope cacheWriteScope,
    IOrderCacheWriter orderCacheWriter,
    IDateTimeProvider dateTimeProvider,
    ILogger<OrderPlacedIntegrationEventHandler> logger)
    : IntegrationEventHandler<OrderPlacedIntegrationEvent>
{
    public override async Task HandleAsync(
        OrderPlacedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        using var _ = cacheWriteScope.AllowWrites();

        logger.LogInformation(
            "Processing OrderPlaced: PublicOrderId={PublicOrderId}, PublicCustomerId={PublicCustomerId}",
            integrationEvent.PublicOrderId,
            integrationEvent.PublicCustomerId);

        var orderCache = new OrderCache
        {
            RefPublicId = integrationEvent.PublicOrderId,
            RefPublicCustomerId = integrationEvent.PublicCustomerId,
            TotalPrice = integrationEvent.TotalPrice,
            Currency = integrationEvent.Currency,
            Status = integrationEvent.Status,
            OrderedAtUtc = integrationEvent.OrderedAtUtc,
            LastSyncedAtUtc = dateTimeProvider.UtcNow
        };

        await orderCacheWriter.UpsertAsync(orderCache, cancellationToken);
    }
}
