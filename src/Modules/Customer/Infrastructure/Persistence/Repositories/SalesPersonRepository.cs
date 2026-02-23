using Microsoft.EntityFrameworkCore;
using Modules.Customer.Domain.SalesPersons;
using Rtl.Core.Infrastructure.Persistence;
using System.Linq.Expressions;

namespace Modules.Customer.Infrastructure.Persistence.Repositories;

internal sealed class SalesPersonRepository(CustomerDbContext dbContext)
    : ReadRepository<SalesPerson, string, CustomerDbContext>(dbContext), ISalesPersonRepository
{
    protected override Expression<Func<SalesPerson, string>> IdSelector => entity => entity.Id;

    public void Add(SalesPerson salesPerson)
        => DbSet.Add(salesPerson);

    public void Update(SalesPerson salesPerson)
        => DbSet.Update(salesPerson);

    public override async Task<SalesPerson?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(sp => sp.Id == id, cancellationToken);
    }
}
