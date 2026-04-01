using Microsoft.EntityFrameworkCore;
using Modules.SampleSales.Domain.Catalogs;
using ModularTemplate.Infrastructure.Persistence;
using System.Linq.Expressions;

namespace Modules.SampleSales.Infrastructure.Persistence.Repositories;

internal sealed class CatalogRepository(SampleDbContext dbContext)
    : Repository<Catalog, int, SampleDbContext>(dbContext), ICatalogRepository
{
    protected override Expression<Func<Catalog, int>> IdSelector => entity => entity.Id;

    public override async Task<IReadOnlyCollection<Catalog>> GetAllAsync(int? limit = 100, CancellationToken cancellationToken = default)
    {
        IQueryable<Catalog> query = DbSet.AsNoTracking().OrderByDescending(c => c.CreatedAtUtc);

        if (limit.HasValue)
        {
            query = query.Take(limit.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }
}
