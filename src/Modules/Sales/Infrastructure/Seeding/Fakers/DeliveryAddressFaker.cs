using Modules.Sales.Domain.DeliveryAddresses;

namespace Modules.Sales.Infrastructure.Seeding.Fakers;

internal static class DeliveryAddressFaker
{
    private static readonly string[] OccupancyTypes = ["Primary Residence", "Secondary Home", "Investment", "Rental"];

    public static List<DeliveryAddress> Generate(int[] saleIds, Bogus.Faker faker, int count = 10)
    {
        var addresses = new List<DeliveryAddress>();
        var selected = faker.PickRandom(saleIds, Math.Min(count, saleIds.Length)).Distinct().ToArray();

        foreach (var saleId in selected)
        {
            var address = DeliveryAddress.Create(
                saleId: saleId,
                occupancyType: faker.PickRandom(OccupancyTypes),
                isWithinCityLimits: faker.Random.Bool(0.6f),
                addressStyle: "Standard",
                addressType: "Delivery",
                addressLine1: faker.Address.StreetAddress(),
                addressLine2: faker.Random.Bool(0.2f) ? faker.Address.SecondaryAddress() : null,
                addressLine3: null,
                city: faker.Address.City(),
                county: faker.Address.County(),
                state: faker.Address.StateAbbr(),
                country: "US",
                postalCode: faker.Address.ZipCode("#####"));

            address.ClearDomainEvents();
            addresses.Add(address);
        }

        return addresses;
    }
}
