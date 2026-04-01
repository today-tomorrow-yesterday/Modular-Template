using Modules.SampleOrders.Domain;
using Modules.SampleOrders.Domain.Orders;
using Modules.SampleOrders.Domain.ProductsCache;
using ModularTemplate.Application.Messaging;
using ModularTemplate.Application.Persistence;
using ModularTemplate.Domain.Results;
using ModularTemplate.Domain.ValueObjects;

namespace Modules.SampleOrders.Application.Orders.PlaceOrder;

internal sealed class PlaceOrderCommandHandler(
    IOrderRepository orderRepository,
    IProductCacheRepository productCacheRepository,
    IUnitOfWork<ISampleOrdersModule> unitOfWork)
    : ICommandHandler<PlaceOrderCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        PlaceOrderCommand request,
        CancellationToken cancellationToken)
    {
        // Get product from local cache (synced from Sales module)
        var product = await productCacheRepository.GetByIdAsync(
            request.ProductCacheId,
            cancellationToken);

        if (product is null || !product.IsActive)
        {
            return Result.Failure<Guid>(OrderErrors.ProductNotFound);
        }

        var orderResult = Order.Place(request.CustomerId);

        if (orderResult.IsFailure)
        {
            return Result.Failure<Guid>(orderResult.Error);
        }

        var order = orderResult.Value;

        // Create Money from product price
        var unitPriceResult = Money.Create(product.Price);
        if (unitPriceResult.IsFailure)
        {
            return Result.Failure<Guid>(unitPriceResult.Error);
        }

        var details = new ProductLineDetails
        {
            ProductName = product.Name
        };

        var addLineResult = order.AddProductLine(
            request.Quantity,
            unitPriceResult.Value,
            request.ProductCacheId,
            details);

        if (addLineResult.IsFailure)
        {
            return Result.Failure<Guid>(addLineResult.Error);
        }

        orderRepository.Add(order);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return order.PublicId;
    }
}
