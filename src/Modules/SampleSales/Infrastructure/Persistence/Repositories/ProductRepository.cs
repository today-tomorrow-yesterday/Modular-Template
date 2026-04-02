using Microsoft.EntityFrameworkCore;
using Modules.SampleSales.Domain.Products;
using ModularTemplate.Application.Exceptions;
using ModularTemplate.Infrastructure.Persistence;
using System.Linq.Expressions;

namespace Modules.SampleSales.Infrastructure.Persistence.Repositories;

internal sealed class ProductRepository(SampleDbContext dbContext)
    : Repository<Product, int, SampleDbContext>(dbContext), IProductRepository
{
    protected override Expression<Func<Product, int>> IdSelector => entity => entity.Id;

    public override async Task<IReadOnlyCollection<Product>> GetAllAsync(int? limit = 100, CancellationToken cancellationToken = default)
    {
        IQueryable<Product> query = DbSet.AsNoTracking().OrderByDescending(p => p.CreatedAtUtc);

        if (limit.HasValue)
        {
            query = query.Take(limit.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<Product> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(p => p.PublicId == publicId, cancellationToken)
            ?? throw new EntityNotFoundException(ProductErrors.NotFound(publicId));
    }
}
