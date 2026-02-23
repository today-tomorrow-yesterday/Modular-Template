using Rtl.Core.Domain;

namespace Modules.Sales.Domain.Sales;

public interface ISaleRepository : IRepository<Sale, int>
{
    Task<Sale?> GetByPartyIdAsync(int partyId, CancellationToken cancellationToken = default);

    Task<Sale?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default);

    Task<Sale?> GetByPublicIdWithDeliveryAddressAsync(Guid publicId, CancellationToken cancellationToken = default);

    Task<Sale?> GetByPublicIdWithFullContextAsync(Guid publicId, CancellationToken cancellationToken = default);

    Task<Sale?> GetByPublicIdWithPartyContextAsync(Guid publicId, CancellationToken cancellationToken = default);

    Task<Sale?> GetByIdWithContextAsync(int id, CancellationToken cancellationToken = default);
}
