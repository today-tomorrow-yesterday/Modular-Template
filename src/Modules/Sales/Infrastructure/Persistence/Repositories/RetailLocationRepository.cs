using Microsoft.EntityFrameworkCore;
using Modules.Sales.Domain.RetailLocations;
using Rtl.Core.Infrastructure.Persistence;
using System.Linq.Expressions;

namespace Modules.Sales.Infrastructure.Persistence.Repositories;

internal sealed class RetailLocationRepository(SalesDbContext dbContext)
    : Repository<RetailLocation, int, SalesDbContext>(dbContext), IRetailLocationRepository
{
    protected override Expression<Func<RetailLocation, int>> IdSelector => entity => entity.Id;

    public async Task<RetailLocation?> GetByHomeCenterNumberAsync(int homeCenterNumber, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RefHomeCenterNumber == homeCenterNumber, cancellationToken);
    }
}
