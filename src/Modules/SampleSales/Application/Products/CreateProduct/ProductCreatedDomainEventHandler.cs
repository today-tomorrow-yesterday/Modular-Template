using Modules.SampleSales.Domain.Products;
using Modules.SampleSales.Domain.Products.Events;
using Modules.SampleSales.IntegrationEvents;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;

namespace Modules.SampleSales.Application.Products.CreateProduct;

internal sealed class ProductCreatedDomainEventHandler(
    IProductRepository productRepository,
    IEventBus eventBus,
    IDateTimeProvider dateTimeProvider) : DomainEventHandler<ProductCreatedDomainEvent>
{
    public override async Task Handle(
        ProductCreatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        Product? product = await productRepository.GetByIdAsync(
            domainEvent.EntityId,
            cancellationToken);

        if (product is null)
        {
            return;
        }

        await eventBus.PublishAsync(
            new ProductCreatedIntegrationEvent(
                Guid.NewGuid(),
                dateTimeProvider.UtcNow,
                product.Id,
                product.Name,
                product.Description,
                product.Price.Amount),
            cancellationToken);
    }
}
