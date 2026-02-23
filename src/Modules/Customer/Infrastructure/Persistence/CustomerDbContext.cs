using Microsoft.EntityFrameworkCore;
using Modules.Customer.Domain;
using Modules.Customer.Domain.Parties.Entities;
using Modules.Customer.Domain.SalesPersons;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Infrastructure.Persistence;

namespace Modules.Customer.Infrastructure.Persistence;

public sealed class CustomerDbContext(DbContextOptions<CustomerDbContext> options)
    : ModuleDbContext<CustomerDbContext>(options), IUnitOfWork<ICustomerModule>
{
    internal DbSet<SalesPerson> SalesPersons => Set<SalesPerson>();

    // Party model — TPH inheritance (single "parties" table, "party_type" discriminator)
    internal DbSet<Party> Parties => Set<Party>();
    internal DbSet<Person> Persons => Set<Person>();
    internal DbSet<Organization> Organizations => Set<Organization>();
    internal DbSet<ContactPoint> ContactPoints => Set<ContactPoint>();
    internal DbSet<PartyIdentifier> PartyIdentifiers => Set<PartyIdentifier>();

    protected override string Schema => Schemas.Customers;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CustomerDbContext).Assembly);
    }
}
