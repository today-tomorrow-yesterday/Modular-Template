using Microsoft.Extensions.Logging;
using Modules.SampleOrders.Domain.ProductsCache;
using Modules.SampleSales.IntegrationEvents;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Domain;

namespace Modules.SampleOrders.Presentation.IntegrationEvents;

internal sealed class ProductUpdatedIntegrationEventHandler(
    ICacheWriteScope cacheWriteScope,
    IProductCacheWriter productCacheWriter,
    IDateTimeProvider dateTimeProvider,
    ILogger<ProductUpdatedIntegrationEventHandler> logger)
    : IntegrationEventHandler<ProductUpdatedIntegrationEvent>
{
    public override async Task HandleAsync(
        ProductUpdatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        using var _ = cacheWriteScope.AllowWrites();

        logger.LogInformation(
            "Processing ProductUpdated: PublicProductId={PublicProductId}, Name={Name}, IsActive={IsActive}",
            integrationEvent.PublicProductId,
            integrationEvent.Name,
            integrationEvent.IsActive);

        var productCache = new ProductCache
        {
            RefPublicId = integrationEvent.PublicProductId,
            Name = integrationEvent.Name,
            Description = integrationEvent.Description,
            Price = integrationEvent.Price,
            IsActive = integrationEvent.IsActive,
            LastSyncedAtUtc = dateTimeProvider.UtcNow
        };

        await productCacheWriter.UpsertAsync(productCache, cancellationToken);
    }
}
