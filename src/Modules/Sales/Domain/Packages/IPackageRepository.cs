using Rtl.Core.Domain;

namespace Modules.Sales.Domain.Packages;

public interface IPackageRepository : IRepository<Package, int>
{
    Task<Package?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Package>> GetBySaleIdAsync(int saleId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Package>> GetBySaleIdWithTrackingAsync(int saleId, CancellationToken cancellationToken = default);
    Task<Package?> GetByPublicIdWithSaleContextAsync(Guid publicId, CancellationToken cancellationToken = default);
    Task<Package?> GetByIdWithSaleContextAsync(int id, CancellationToken cancellationToken = default);
}
