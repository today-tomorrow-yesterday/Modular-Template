using Microsoft.EntityFrameworkCore;
using Modules.Sales.Domain.Packages;
using Rtl.Core.Infrastructure.Persistence;
using System.Linq.Expressions;

namespace Modules.Sales.Infrastructure.Persistence.Repositories;

internal sealed class PackageRepository(SalesDbContext dbContext)
    : Repository<Package, int, SalesDbContext>(dbContext), IPackageRepository
{
    protected override Expression<Func<Package, int>> IdSelector => entity => entity.Id;

    public override async Task<Package?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().Include(p => p.Lines)
                      .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public override async Task<IReadOnlyCollection<Package>> GetAllAsync(int? limit = 100, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().Include(p => p.Lines).ToListAsync(cancellationToken);

    public async Task<Package?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.PublicId == publicId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Package>> GetBySaleIdAsync(int saleId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Lines)
            .AsNoTracking()
            .Where(p => p.SaleId == saleId)
            .OrderBy(p => p.Ranking)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Package>> GetBySaleIdWithTrackingAsync(int saleId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Lines)
            .Where(p => p.SaleId == saleId)
            .OrderBy(p => p.Ranking)
            .ToListAsync(cancellationToken);
    }

    public async Task<Package?> GetByPublicIdWithSaleContextAsync(Guid publicId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Lines)
            .Include(p => p.Sale).ThenInclude(s => s.RetailLocation)
            .Include(p => p.Sale).ThenInclude(s => s.DeliveryAddress)
            .FirstOrDefaultAsync(p => p.PublicId == publicId, cancellationToken);
    }

    public async Task<Package?> GetByIdWithSaleContextAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Lines)
            .Include(p => p.Sale).ThenInclude(s => s.RetailLocation)
            .Include(p => p.Sale).ThenInclude(s => s.DeliveryAddress)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
}
