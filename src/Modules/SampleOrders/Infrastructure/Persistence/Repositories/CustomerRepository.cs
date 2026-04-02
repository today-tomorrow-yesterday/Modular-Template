using Microsoft.EntityFrameworkCore;
using Modules.SampleOrders.Domain.Customers;
using ModularTemplate.Application.Exceptions;
using ModularTemplate.Infrastructure.Persistence;
using System.Linq.Expressions;

namespace Modules.SampleOrders.Infrastructure.Persistence.Repositories;

internal sealed class CustomerRepository(OrdersDbContext dbContext)
    : Repository<Customer, int, OrdersDbContext>(dbContext), ICustomerRepository
{
    protected override Expression<Func<Customer, int>> IdSelector => entity => entity.Id;

    public override async Task<IReadOnlyCollection<Customer>> GetAllAsync(int? limit = 100, CancellationToken cancellationToken = default)
    {
        IQueryable<Customer> query = DbSet.AsNoTracking().OrderByDescending(c => c.CreatedAtUtc);

        if (limit.HasValue)
        {
            query = query.Take(limit.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<Customer> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(c => c.PublicId == publicId, cancellationToken)
            ?? throw new EntityNotFoundException(CustomerErrors.NotFound(publicId));
    }

    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .FirstOrDefaultAsync(
                c => c.Contacts.Any(ct => ct.Type == ContactType.Email && ct.Value == email),
                cancellationToken);
    }
}
