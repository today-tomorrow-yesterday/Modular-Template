using Microsoft.Extensions.Logging;
using Modules.SampleOrders.Domain.ProductsCache;
using Modules.SampleSales.IntegrationEvents;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Domain;

namespace Modules.SampleOrders.Presentation.IntegrationEvents;

/// <summary>
/// Handles ProductCreatedIntegrationEvent from the Sales module.
/// Upserts product data into the local ProductCache for read operations.
/// </summary>
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
            "Processing ProductCreated integration event: ProductId={ProductId}, Name={Name}",
            integrationEvent.ProductId,
            integrationEvent.Name);

        var productCache = new ProductCache
        {
            Id = integrationEvent.ProductId,
            Name = integrationEvent.Name,
            Description = integrationEvent.Description,
            Price = integrationEvent.Price,
            IsActive = true,
            LastSyncedAtUtc = dateTimeProvider.UtcNow
        };

        await productCacheWriter.UpsertAsync(productCache, cancellationToken);
    }
}

/// <summary>
/// Interface for writing to the ProductCache.
/// Used only by integration event handlers.
/// </summary>
public interface IProductCacheWriter
{
    Task UpsertAsync(ProductCache productCache, CancellationToken cancellationToken = default);
}
