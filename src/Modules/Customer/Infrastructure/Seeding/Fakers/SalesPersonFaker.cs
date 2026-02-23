using Bogus;
using Modules.Customer.Domain.SalesPersons;

namespace Modules.Customer.Infrastructure.Seeding.Fakers;

internal static class SalesPersonFaker
{
    private static readonly string[] FederatedIds =
    [
        "auth0|sp001", "auth0|sp002", "auth0|sp003",
        "auth0|sp004", "auth0|sp005", "auth0|sp006"
    ];

    public static List<SalesPerson> Generate()
    {
        var faker = new Faker();
        var salesPersons = new List<SalesPerson>();

        for (var i = 0; i < FederatedIds.Length; i++)
        {
            var firstName = faker.Name.FirstName();
            var lastName = faker.Name.LastName();
            var lotNumber = (i / 2) + 1; // 2 SPs per lot

            salesPersons.Add(SalesPerson.Assign(
                id: $"SF-SP-{(i + 1):D4}",
                email: faker.Internet.Email(firstName, lastName, "cmhomes.com"),
                username: $"{firstName.ToLower()}.{lastName.ToLower()}",
                firstName: firstName,
                lastName: lastName,
                lotNumber: lotNumber,
                federatedId: FederatedIds[i]));
        }

        return salesPersons;
    }
}
