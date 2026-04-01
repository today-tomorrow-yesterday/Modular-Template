using Modules.SampleSales.Domain.Products;
using Modules.SampleSales.Domain.Products.Events;
using Modules.SampleSales.IntegrationEvents;
using ModularTemplate.Application.EventBus;
using ModularTemplate.Application.Messaging;
using ModularTemplate.Domain;

namespace Modules.SampleSales.Application.Products.UpdateProduct;

internal sealed class ProductUpdatedDomainEventHandler(
    IProductRepository productRepository,
    IEventBus eventBus,
    IDateTimeProvider dateTimeProvider) : DomainEventHandler<ProductUpdatedDomainEvent>
{
    public override async Task Handle(
        ProductUpdatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(
            domainEvent.EntityId,
            cancellationToken);

        if (product is null) return;

        await eventBus.PublishAsync(
            new ProductUpdatedIntegrationEvent(
                Guid.CreateVersion7(),
                dateTimeProvider.UtcNow,
                product.PublicId,
                product.Name,
                product.Description,
                product.Price.Amount,
                product.IsActive),
            cancellationToken);
    }
}
