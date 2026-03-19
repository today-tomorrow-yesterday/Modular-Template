using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modules.Customer.Infrastructure.Persistence;
using Modules.Customer.Infrastructure.Seeding.Fakers;
using Rtl.Core.Application.Seeding;

namespace Modules.Customer.Infrastructure.Seeding;

internal sealed class CustomerModuleSeeder : IModuleSeeder
{
    public string ModuleName => "Customer";
    public int Order => 20;

    public async Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        var db = services.GetRequiredService<CustomerDbContext>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger<CustomerModuleSeeder>();

        // Idempotency check
        if (await db.Customers.AnyAsync(ct))
        {
            logger.LogInformation("Customer module already has data. Skipping seed.");
            return;
        }

        var faker = new Bogus.Faker();

        // ────────────────────────────────────────
        // Phase 1: SalesPersons (FK target — must exist before SalesAssignments)
        // ────────────────────────────────────────

        var salesPersons = SalesPersonFaker.Generate();
        db.SalesPersons.AddRange(salesPersons);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seeded {Count} salespersons.", salesPersons.Count);

        var salesPersonIds = salesPersons.Select(sp => sp.Id).ToArray();

        // ────────────────────────────────────────
        // Phase 2: Customers
        // ────────────────────────────────────────

        var customers = CustomerFaker.Generate(15, salesPersonIds, faker);
        foreach (var c in customers) c.ClearDomainEvents();
        db.Customers.AddRange(customers);

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seeded {Count} customers.", customers.Count);
    }
}
