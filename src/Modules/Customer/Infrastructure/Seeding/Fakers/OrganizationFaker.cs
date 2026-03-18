using Bogus;
using Modules.Customer.Domain.Parties.Entities;
using Modules.Customer.Domain.Parties.Enums;

namespace Modules.Customer.Infrastructure.Seeding.Fakers;

internal static class OrganizationFaker
{
    public static List<Organization> Generate(int startId, Faker faker)
    {
        var orgs = new List<Organization>();
        var orgNames = new[]
        {
            "Sunrise Development Group",
            "HomeFirst Communities LLC",
            "Valley Land Partners Inc."
        };

        for (var i = 0; i < orgNames.Length; i++)
        {
            var stage = faker.PickRandom<LifecycleStage>();
            var homeCenterNumber = faker.PickRandom(100, 200, 300, 400, 500);

            var org = Organization.SyncFromCrm(
                crmPartyId: startId + i,
                homeCenterNumber: homeCenterNumber,
                lifecycleStage: stage,
                organizationName: orgNames[i],
                salesforceUrl: $"https://cmhomes.lightning.force.com/lightning/r/Account/{faker.Random.AlphaNumeric(18)}/view",
                mailingAddress: MailingAddress.Create(
                    addressLine1: faker.Address.StreetAddress(),
                    addressLine2: null,
                    city: faker.Address.City(),
                    county: faker.Address.County(),
                    state: faker.Address.StateAbbr(),
                    country: "US",
                    postalCode: faker.Address.ZipCode()),
                createdOn: faker.Date.RecentOffset(120).ToUniversalTime(),
                lastModifiedOn: faker.Date.RecentOffset(30).ToUniversalTime());

            orgs.Add(org);
        }

        return orgs;
    }
}
