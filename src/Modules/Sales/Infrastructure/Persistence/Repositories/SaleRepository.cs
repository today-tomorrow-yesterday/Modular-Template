using Microsoft.EntityFrameworkCore;
using Modules.Sales.Domain.Sales;
using Rtl.Core.Infrastructure.Persistence;
using System.Linq.Expressions;

namespace Modules.Sales.Infrastructure.Persistence.Repositories;

internal sealed class SaleRepository(SalesDbContext dbContext)
    : Repository<Sale, int, SalesDbContext>(dbContext), ISaleRepository
{
    protected override Expression<Func<Sale, int>> IdSelector => entity => entity.Id;

    public async Task<Sale?> GetByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.CustomerId == customerId, cancellationToken);
    }

    public async Task<Sale?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.PublicId == publicId, cancellationToken);
    }

    public async Task<Sale?> GetByPublicIdWithDeliveryAddressAsync(Guid publicId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.DeliveryAddress)
            .FirstOrDefaultAsync(s => s.PublicId == publicId, cancellationToken);
    }

    public async Task<Sale?> GetByPublicIdWithFullContextAsync(Guid publicId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.RetailLocation)
            .Include(s => s.DeliveryAddress)
            .Include(s => s.Customer)
            .Include(s => s.Packages).ThenInclude(p => p.Lines)
            .FirstOrDefaultAsync(s => s.PublicId == publicId, cancellationToken);
    }

    public async Task<Sale?> GetByPublicIdWithCustomerContextAsync(Guid publicId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.RetailLocation)
            .FirstOrDefaultAsync(s => s.PublicId == publicId, cancellationToken);
    }

    public async Task<Sale?> GetByIdWithContextAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.RetailLocation)
            .Include(s => s.Packages).ThenInclude(p => p.Lines)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }
}
