using Modules.Sales.Domain.Packages;

namespace Modules.Sales.Infrastructure.Seeding.Fakers;

internal static class PackageFaker
{
    public static List<Package> Generate(int[] saleIds, Bogus.Faker faker)
    {
        var packages = new List<Package>();

        foreach (var saleId in saleIds)
        {
            // Every sale gets a primary package
            var primary = Package.Create(saleId, "Primary", isPrimary: true);
            primary.ClearDomainEvents();
            packages.Add(primary);

            // ~40% of sales get an alternate package
            if (faker.Random.Bool(0.4f))
            {
                var alternate = Package.Create(saleId, "Alternate", isPrimary: false);
                alternate.SetNonPrimary(2);
                alternate.ClearDomainEvents();
                packages.Add(alternate);
            }
        }

        return packages;
    }
}
