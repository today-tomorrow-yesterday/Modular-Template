using Microsoft.EntityFrameworkCore;
using Modules.Customer.Domain.Customers;
using Modules.Customer.Domain.Customers.Enums;
using Rtl.Core.Infrastructure.Persistence;
using System.Linq.Expressions;

namespace Modules.Customer.Infrastructure.Persistence.Repositories;

internal sealed class CustomerRepository(CustomerDbContext dbContext)
    : Repository<Domain.Customers.Entities.Customer, int, CustomerDbContext>(dbContext), ICustomerRepository
{
    protected override Expression<Func<Domain.Customers.Entities.Customer, int>> IdSelector => entity => entity.Id;

    public override async Task<Domain.Customers.Entities.Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.ContactPoints)
            .Include(c => c.Identifiers)
            .Include(c => c.SalesAssignments)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Domain.Customers.Entities.Customer?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default)
    {
        return await ReadOnlyDetailQuery()
            .FirstOrDefaultAsync(c => c.PublicId == publicId, cancellationToken);
    }

    public async Task<Domain.Customers.Entities.Customer?> GetByIdentifierAsync(
        IdentifierType type, string value, CancellationToken cancellationToken = default)
    {
        return await ReadOnlyDetailQuery()
            .FirstOrDefaultAsync(c => c.Identifiers.Any(i => i.Type == type && i.Value == value), cancellationToken);
    }

    public async Task<Domain.Customers.Entities.Customer?> GetForUpdateByIdentifierAsync(
        IdentifierType type, string value, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.ContactPoints)
            .Include(c => c.Identifiers)
            .Include(c => c.SalesAssignments)
            .FirstOrDefaultAsync(c => c.Identifiers.Any(i => i.Type == type && i.Value == value), cancellationToken);
    }

    public async Task<Domain.Customers.Entities.Customer?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await ReadOnlyDetailQuery()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    private IQueryable<Domain.Customers.Entities.Customer> ReadOnlyDetailQuery()
    {
        return DbSet
            .Include(c => c.ContactPoints)
            .Include(c => c.Identifiers)
            .Include(c => c.SalesAssignments)
                .ThenInclude(sa => sa.SalesPerson)
            .Include(c => c.CoBuyer)
            .AsNoTracking();
    }
}
