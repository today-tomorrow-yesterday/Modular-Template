using Modules.Sales.Domain.DeliveryAddresses;
using Rtl.Core.Infrastructure.Persistence;
using System.Linq.Expressions;

namespace Modules.Sales.Infrastructure.Persistence.Repositories;

internal sealed class DeliveryAddressRepository(SalesDbContext dbContext)
    : Repository<DeliveryAddress, int, SalesDbContext>(dbContext), IDeliveryAddressRepository
{
    protected override Expression<Func<DeliveryAddress, int>> IdSelector => entity => entity.Id;
}
