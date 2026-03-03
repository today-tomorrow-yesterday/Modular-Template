using Microsoft.EntityFrameworkCore;
using Modules.Sales.Domain.DeliveryAddresses;
using Rtl.Core.Infrastructure.Persistence;
using System.Linq.Expressions;

namespace Modules.Sales.Infrastructure.Persistence.Repositories;

internal sealed class DeliveryAddressRepository(SalesDbContext dbContext)
    : Repository<DeliveryAddress, int, SalesDbContext>(dbContext), IDeliveryAddressRepository
{
    protected override Expression<Func<DeliveryAddress, int>> IdSelector => entity => entity.Id;

    public override async Task<DeliveryAddress?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(d => d.Sale)
                .ThenInclude(s => s.Party)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }
}
