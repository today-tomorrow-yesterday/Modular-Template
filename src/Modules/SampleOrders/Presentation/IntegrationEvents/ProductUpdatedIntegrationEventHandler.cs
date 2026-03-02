using Microsoft.Extensions.Logging;
using Modules.SampleOrders.Domain.ProductsCache;
using Modules.SampleSales.IntegrationEvents;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Domain;

namespace Modules.SampleOrders.Presentation.IntegrationEvents;

/// <summary>
/// Handles ProductUpdatedIntegrationEvent from the Sales module.
/// Updates the product data in the local ProductCache.
/// </summary>
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
            "Processing ProductUpdated integration event: ProductId={ProductId}, Name={Name}, IsActive={IsActive}",
            integrationEvent.ProductId,
            integrationEvent.Name,
            integrationEvent.IsActive);

        var productCache = new ProductCache
        {
            Id = integrationEvent.ProductId,
            Name = integrationEvent.Name,
            Description = integrationEvent.Description,
            Price = integrationEvent.Price,
            IsActive = integrationEvent.IsActive,
            LastSyncedAtUtc = dateTimeProvider.UtcNow
        };

        await productCacheWriter.UpsertAsync(productCache, cancellationToken);
    }
}
