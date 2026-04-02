using ModularTemplate.Domain;

namespace Modules.SampleSales.Domain.Products;

public interface IProductRepository : IRepository<Product, int>
{
    Task<Product> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default);
}
