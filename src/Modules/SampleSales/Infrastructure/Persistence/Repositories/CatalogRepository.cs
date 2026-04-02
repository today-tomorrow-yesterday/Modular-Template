using Microsoft.EntityFrameworkCore;
using Modules.SampleSales.Domain.Catalogs;
using ModularTemplate.Application.Exceptions;
using ModularTemplate.Infrastructure.Persistence;
using System.Linq.Expressions;

namespace Modules.SampleSales.Infrastructure.Persistence.Repositories;

internal sealed class CatalogRepository(SampleDbContext dbContext)
    : Repository<Catalog, int, SampleDbContext>(dbContext), ICatalogRepository
{
    protected override Expression<Func<Catalog, int>> IdSelector => entity => entity.Id;

    public override async Task<IReadOnlyCollection<Catalog>> GetAllAsync(int? limit = 100, CancellationToken cancellationToken = default)
    {
        return await GetAllAsync(limit, offset: 0, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Catalog>> GetAllAsync(int? limit, int offset, CancellationToken cancellationToken = default)
    {
        IQueryable<Catalog> query = DbSet.AsNoTracking().OrderByDescending(c => c.CreatedAtUtc);

        if (offset > 0)
        {
            query = query.Skip(offset);
        }

        if (limit.HasValue)
        {
            query = query.Take(limit.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<Catalog> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.PublicId == publicId, cancellationToken)
            ?? throw new EntityNotFoundException(CatalogErrors.NotFound(publicId));
    }
}
