using Microsoft.EntityFrameworkCore;
using Modules.Customer.Domain;
using Modules.Customer.Domain.Customers.Entities;
using Modules.Customer.Domain.SalesPersons;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Infrastructure.Persistence;

namespace Modules.Customer.Infrastructure.Persistence;

public sealed class CustomerDbContext(DbContextOptions<CustomerDbContext> options)
    : ModuleDbContext<CustomerDbContext>(options), IUnitOfWork<ICustomerModule>
{
    internal DbSet<SalesPerson> SalesPersons => Set<SalesPerson>();

    internal DbSet<Domain.Customers.Entities.Customer> Customers => Set<Domain.Customers.Entities.Customer>();
    internal DbSet<ContactPoint> ContactPoints => Set<ContactPoint>();
    internal DbSet<CustomerIdentifier> CustomerIdentifiers => Set<CustomerIdentifier>();

    protected override string Schema => Schemas.Customers;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CustomerDbContext).Assembly);
    }
}
