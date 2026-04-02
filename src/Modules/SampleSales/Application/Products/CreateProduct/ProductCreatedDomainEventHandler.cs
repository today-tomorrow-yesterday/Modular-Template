using Microsoft.Extensions.Logging;
using Modules.SampleSales.Domain.Products;
using Modules.SampleSales.Domain.Products.Events;
using Modules.SampleSales.IntegrationEvents;
using ModularTemplate.Application.EventBus;
using ModularTemplate.Application.Messaging;
using ModularTemplate.Domain;

namespace Modules.SampleSales.Application.Products.CreateProduct;

internal sealed class ProductCreatedDomainEventHandler(
    IProductRepository productRepository,
    IEventBus eventBus,
    IDateTimeProvider dateTimeProvider,
    ILogger<ProductCreatedDomainEventHandler> logger) : DomainEventHandler<ProductCreatedDomainEvent>
{
    public override async Task Handle(
        ProductCreatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(
            domainEvent.EntityId,
            cancellationToken);

        if (product is null)
        {
            logger.LogWarning(
                "Product with EntityId={EntityId} not found when handling ProductCreatedDomainEvent. Skipping integration event publish.",
                domainEvent.EntityId);
            return;
        }

        await eventBus.PublishAsync(
            new ProductCreatedIntegrationEvent(
                Guid.CreateVersion7(),
                dateTimeProvider.UtcNow,
                product.PublicId,
                product.Name,
                product.Description,
                product.Price.Amount),
            cancellationToken);
    }
}
