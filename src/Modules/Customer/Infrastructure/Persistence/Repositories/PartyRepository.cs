using Microsoft.EntityFrameworkCore;
using Modules.Customer.Domain.Parties;
using Modules.Customer.Domain.Parties.Entities;
using Modules.Customer.Domain.Parties.Enums;
using Rtl.Core.Infrastructure.Persistence;
using System.Linq.Expressions;

namespace Modules.Customer.Infrastructure.Persistence.Repositories;

internal sealed class PartyRepository(CustomerDbContext dbContext)
    : Repository<Party, int, CustomerDbContext>(dbContext), IPartyRepository
{
    protected override Expression<Func<Party, int>> IdSelector => entity => entity.Id;

    // Tracking query — command handlers need this for updates.
    // Includes SalesAssignments so UpdateFromCrmSync replace-all semantics work correctly.
    // Note: The cast to Person in Include is the EF Core TPH (Table Per Hierarchy) pattern — EF translates it as a
    // conditional left-join that only applies to Person rows. Safe with TPH (Table Per Hierarchy),
    // not with TPT (Table Per Type) / TPC (Table Per Concrete type).
    public override async Task<Party?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.ContactPoints)
            .Include(p => p.Identifiers)
            .Include(p => ((Person)p).SalesAssignments)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Party?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default)
    {
        return await ReadOnlyDetailQuery()
            .FirstOrDefaultAsync(p => p.PublicId == publicId, cancellationToken);
    }

    public async Task<Party?> GetByIdentifierAsync(
        IdentifierType type,
        string value,
        CancellationToken cancellationToken = default)
    {
        return await ReadOnlyDetailQuery()
            .FirstOrDefaultAsync(
                p => p.Identifiers.Any(i => i.Type == type && i.Value == value),
                cancellationToken);
    }

    public async Task<Party?> GetForUpdateByIdentifierAsync(
        IdentifierType type,
        string value,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.ContactPoints)
            .Include(p => p.Identifiers)
            .Include(p => ((Person)p).SalesAssignments)
            .FirstOrDefaultAsync(
                p => p.Identifiers.Any(i => i.Type == type && i.Value == value),
                cancellationToken);
    }

    public async Task<Party?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await ReadOnlyDetailQuery()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    private IQueryable<Party> ReadOnlyDetailQuery()
    {
        return DbSet
            .Include(p => p.ContactPoints)
            .Include(p => p.Identifiers)
            .Include(p => ((Person)p).SalesAssignments)
                .ThenInclude(sa => sa.SalesPerson)
            .Include(p => ((Person)p).CoBuyer)
            .AsNoTracking();
    }
}
