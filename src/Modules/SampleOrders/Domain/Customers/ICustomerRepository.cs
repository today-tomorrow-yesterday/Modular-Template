using ModularTemplate.Domain;

namespace Modules.SampleOrders.Domain.Customers;

public interface ICustomerRepository : IRepository<Customer, int>
{
    Task<Customer> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default);

    Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
