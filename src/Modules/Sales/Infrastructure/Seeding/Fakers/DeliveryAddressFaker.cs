using Modules.Sales.Domain.DeliveryAddresses;

namespace Modules.Sales.Infrastructure.Seeding.Fakers;

internal static class DeliveryAddressFaker
{
    private static readonly string[] OccupancyTypes = ["Primary Residence", "Secondary Home", "Investment", "Rental"];

    // Addresses verified against the iSeries tax API (legacy database).
    private static readonly (string City, string County, string State, string Zip)[] VerifiedAddresses =
    [
        ("Maryville", "Blount", "TN", "37801"),
        ("Knoxville", "Knox", "TN", "37920"),
        ("Birmingham", "Jefferson", "AL", "35212"),
        ("Montgomery", "Montgomery", "AL", "36043"),
        ("Hilton Head", "Beaufort", "SC", "29915"),
        ("Tallahassee", "Leon", "FL", "32304"),
        ("Caryville", "Campbell", "TN", "37714"),
        ("Mascot", "New Market", "TN", "37820"),
        ("Fredericksburg", "Fredericksburg", "VA", "22401"),
        ("Coosa", "Coosa", "AL", "35011"),
    ];

    public static List<DeliveryAddress> Generate(int[] saleIds, Bogus.Faker faker, int count = 10)
    {
        var addresses = new List<DeliveryAddress>();
        var selected = faker.PickRandom(saleIds, Math.Min(count, saleIds.Length)).Distinct().ToArray();

        foreach (var saleId in selected)
        {
            var loc = faker.PickRandom(VerifiedAddresses);

            var address = DeliveryAddress.Create(
                saleId: saleId,
                occupancyType: faker.PickRandom(OccupancyTypes),
                isWithinCityLimits: faker.Random.Bool(0.6f),
                addressStyle: "Standard",
                addressType: "Delivery",
                addressLine1: faker.Address.StreetAddress(),
                addressLine2: faker.Random.Bool(0.2f) ? faker.Address.SecondaryAddress() : null,
                addressLine3: null,
                city: loc.City,
                county: loc.County,
                state: loc.State,
                country: "US",
                postalCode: loc.Zip);

            address.ClearDomainEvents();
            addresses.Add(address);
        }

        return addresses;
    }
}
