using Modules.Customer.Domain.Customers.Enums;
using Rtl.Core.Domain;

namespace Modules.Customer.Domain.Customers;

public interface ICustomerRepository : IRepository<Entities.Customer, int>
{
    Task<Entities.Customer?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default);

    Task<Entities.Customer?> GetByIdentifierAsync(IdentifierType type, string value, CancellationToken cancellationToken = default);

    Task<Entities.Customer?> GetForUpdateByIdentifierAsync(IdentifierType type, string value, CancellationToken cancellationToken = default);

    Task<Entities.Customer?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
}
