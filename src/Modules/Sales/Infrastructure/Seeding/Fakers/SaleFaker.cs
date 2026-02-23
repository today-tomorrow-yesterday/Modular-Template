using Modules.Sales.Domain.Sales;

namespace Modules.Sales.Infrastructure.Seeding.Fakers;

internal static class SaleFaker
{
    private static int _saleNumber = 1000;

    public static List<Sale> Generate(int count, int[] partyIds, int[] retailLocationIds, Bogus.Faker faker)
    {
        var sales = new List<Sale>(count);

        for (var i = 0; i < count; i++)
        {
            var sale = Sale.Create(
                partyId: faker.PickRandom(partyIds),
                retailLocationId: faker.PickRandom(retailLocationIds),
                saleType: faker.PickRandom(SaleType.B2C, SaleType.B2C, SaleType.B2C, SaleType.B2B), // 75% B2C
                saleNumber: ++_saleNumber);

            // Advance some sales beyond Inquiry
            if (i % 3 == 0)
                sale.UpdateStatus(SaleStatus.Discovery);
            else if (i % 5 == 0)
                sale.UpdateStatus(SaleStatus.Application);

            sale.ClearDomainEvents();
            sales.Add(sale);
        }

        return sales;
    }
}
