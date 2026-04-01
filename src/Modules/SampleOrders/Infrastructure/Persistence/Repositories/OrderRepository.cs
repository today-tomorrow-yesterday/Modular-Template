using Microsoft.EntityFrameworkCore;
using Modules.SampleOrders.Domain.Orders;
using ModularTemplate.Infrastructure.Persistence;
using System.Linq.Expressions;

namespace Modules.SampleOrders.Infrastructure.Persistence.Repositories;

internal sealed class OrderRepository(OrdersDbContext dbContext)
    : Repository<Order, int, OrdersDbContext>(dbContext), IOrderRepository
{
    protected override Expression<Func<Order, int>> IdSelector => entity => entity.Id;

    public override async Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public override async Task<IReadOnlyCollection<Order>> GetAllAsync(int? limit = 100, CancellationToken cancellationToken = default)
    {
        IQueryable<Order> query = DbSet
            .Include(o => o.Lines)
            .AsNoTracking()
            .OrderByDescending(o => o.OrderedAtUtc);

        if (limit.HasValue)
        {
            query = query.Take(limit.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Order>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(o => o.Lines)
            .AsNoTracking()
            .Where(o => o.Lines.OfType<ProductLine>().Any(l => l.ProductCacheId == productId))
            .OrderByDescending(o => o.OrderedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Order>> GetByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(o => o.Lines)
            .AsNoTracking()
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.OrderedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
