using ModularTemplate.Domain;

namespace Modules.SampleSales.Domain.Catalogs;

public interface ICatalogRepository : IRepository<Catalog, int>
{
    Task<Catalog> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Catalog>> GetAllAsync(int? limit, int offset, CancellationToken cancellationToken = default);
}
