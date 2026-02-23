using Modules.SampleSales.Domain.Products;
using Modules.SampleSales.Domain.Products.Events;
using Modules.SampleSales.IntegrationEvents;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;

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
        Product? product = await productRepository.GetByIdAsync(
            domainEvent.EntityId,
            cancellationToken);

        if (product is null)
        {
            return;
        }

        await eventBus.PublishAsync(
            new ProductUpdatedIntegrationEvent(
                Guid.NewGuid(),
                dateTimeProvider.UtcNow,
                product.Id,
                product.Name,
                product.Description,
                product.Price.Amount,
                product.IsActive),
            cancellationToken);
    }
}
