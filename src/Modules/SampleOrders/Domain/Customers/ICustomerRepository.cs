using Rtl.Core.Domain;

namespace Modules.SampleOrders.Domain.Customers;

public interface ICustomerRepository : IRepository<Customer, int>
{
    Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
