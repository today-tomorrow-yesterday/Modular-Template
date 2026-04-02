using ModularTemplate.Domain;

namespace Modules.SampleOrders.Domain.Orders;

public interface IOrderRepository : IRepository<Order, int>
{
    Task<Order> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Order>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Order>> GetByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default);
}
