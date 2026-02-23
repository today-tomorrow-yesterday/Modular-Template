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
        if (await db.Parties.AnyAsync(ct))
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
        // Phase 2: Parties (Person + Organization via TPH)
        // ────────────────────────────────────────

        // Persons (partyIds 1..15) — with SalesAssignments created via factory
        var persons = PersonFaker.Generate(15, salesPersonIds, faker);
        foreach (var p in persons) p.ClearDomainEvents();
        db.Persons.AddRange(persons);

        // Organizations (partyIds 16..18) — no SalesAssignments, no CoBuyer
        var orgs = OrganizationFaker.Generate(startId: 16, faker);
        foreach (var o in orgs) o.ClearDomainEvents();
        db.Organizations.AddRange(orgs);

        await db.SaveChangesAsync(ct);
        logger.LogInformation(
            "Seeded {Persons} persons, {Orgs} organizations.",
            persons.Count, orgs.Count);
    }
}
