using Modules.Customer.Domain.Parties.Entities;
using Modules.Customer.Domain.Parties.Enums;

namespace Modules.Customer.Infrastructure.Seeding.Fakers;

internal static class PersonFaker
{
    public static List<Person> Generate(
        int count,
        string[] salesPersonIds,
        Bogus.Faker faker)
    {
        var persons = new List<Person>();

        for (var i = 1; i <= count; i++)
        {
            var firstName = faker.Name.FirstName();
            var lastName = faker.Name.LastName();
            var middleName = faker.Random.Bool(0.3f) ? faker.Name.FirstName() : null;
            var nameExtension = faker.Random.Bool(0.1f) ? faker.PickRandom("Jr.", "Sr.", "III") : null;

            var stage = faker.PickRandom<LifecycleStage>();
            var homeCenterNumber = faker.PickRandom(100, 200, 300, 400, 500);

            // Build sales assignments — most persons have a primary SP
            var assignments = new List<(string SalesPersonId, SalesAssignmentRole Role)>();
            if (salesPersonIds.Length > 0 && faker.Random.Bool(0.85f))
            {
                assignments.Add((faker.PickRandom(salesPersonIds), SalesAssignmentRole.Primary));

                // ~30% also get a supporting SP
                if (faker.Random.Bool(0.3f) && salesPersonIds.Length > 1)
                {
                    var supportingId = faker.PickRandom(
                        salesPersonIds.Where(id => id != assignments[0].SalesPersonId).ToArray());
                    assignments.Add((supportingId, SalesAssignmentRole.Supporting));
                }
            }

            var dob = faker.Random.Bool(0.7f)
                ? DateOnly.FromDateTime(faker.Date.Past(40, DateTime.Now.AddYears(-21)))
                : (DateOnly?)null;

            // Addresses verified against the iSeries tax API (legacy database).
            var mailingLoc = faker.PickRandom(
                ("Maryville", "Blount", "TN", "37801"),
                ("Knoxville", "Knox", "TN", "37920"),
                ("Birmingham", "Jefferson", "AL", "35212"),
                ("Montgomery", "Montgomery", "AL", "36043"),
                ("Hilton Head", "Beaufort", "SC", "29915"),
                ("Tallahassee", "Leon", "FL", "32304"));

            var mailingAddress = faker.Random.Bool(0.6f)
                ? MailingAddress.Create(
                    addressLine1: faker.Address.StreetAddress(),
                    addressLine2: faker.Random.Bool(0.2f) ? faker.Address.SecondaryAddress() : null,
                    city: mailingLoc.Item1,
                    county: mailingLoc.Item2,
                    state: mailingLoc.Item3,
                    country: "US",
                    postalCode: mailingLoc.Item4)
                : null;

            var person = Person.SyncFromCrm(
                partyId: i,
                homeCenterNumber: homeCenterNumber,
                lifecycleStage: stage,
                name: PersonName.Create(firstName, middleName, lastName, nameExtension),
                dateOfBirth: dob,
                salesAssignments: assignments.ToArray(),
                salesforceUrl: $"https://cmhomes.lightning.force.com/lightning/r/Lead/{faker.Random.AlphaNumeric(18)}/view",
                mailingAddress: mailingAddress,
                createdOn: faker.Date.RecentOffset(90).ToUniversalTime(),
                lastModifiedOn: faker.Date.RecentOffset(30).ToUniversalTime());

            persons.Add(person);
        }

        return persons;
    }
}
