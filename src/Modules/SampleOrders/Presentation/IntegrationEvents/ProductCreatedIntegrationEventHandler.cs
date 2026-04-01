using Microsoft.Extensions.Logging;
using Modules.SampleOrders.Domain.ProductsCache;
using Modules.SampleSales.IntegrationEvents;
using ModularTemplate.Application.Caching;
using ModularTemplate.Application.EventBus;
using ModularTemplate.Domain;

namespace Modules.SampleOrders.Presentation.IntegrationEvents;

internal sealed class ProductCreatedIntegrationEventHandler(
    ICacheWriteScope cacheWriteScope,
    IProductCacheWriter productCacheWriter,
    IDateTimeProvider dateTimeProvider,
    ILogger<ProductCreatedIntegrationEventHandler> logger)
    : IntegrationEventHandler<ProductCreatedIntegrationEvent>
{
    public override async Task HandleAsync(
        ProductCreatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        using var _ = cacheWriteScope.AllowWrites();

        logger.LogInformation(
            "Processing ProductCreated: PublicProductId={PublicProductId}, Name={Name}",
            integrationEvent.PublicProductId,
            integrationEvent.Name);

        var productCache = new ProductCache
        {
            RefPublicId = integrationEvent.PublicProductId,
            Name = integrationEvent.Name,
            Description = integrationEvent.Description,
            Price = integrationEvent.Price,
            IsActive = true,
            LastSyncedAtUtc = dateTimeProvider.UtcNow
        };

        await productCacheWriter.UpsertAsync(productCache, cancellationToken);
    }
}
