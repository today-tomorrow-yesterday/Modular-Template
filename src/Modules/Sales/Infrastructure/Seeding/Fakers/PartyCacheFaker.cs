using Bogus;
using Modules.Sales.Domain.PartiesCache;

namespace Modules.Sales.Infrastructure.Seeding.Fakers;

internal sealed class PartyCacheFaker : Faker<PartyCache>
{
    private int _refPartyId;

    public PartyCacheFaker(int[] homeCenterNumbers)
    {
        _refPartyId = 0;

        RuleFor(p => p.RefPartyId, _ => ++_refPartyId);
        RuleFor(p => p.RefPublicId, _ => Guid.CreateVersion7());
        RuleFor(p => p.PartyType, PartyType.Person);
        RuleFor(p => p.LifecycleStage, f => f.PickRandom<LifecycleStage>());
        RuleFor(p => p.HomeCenterNumber, f => f.PickRandom(homeCenterNumbers));
        RuleFor(p => p.DisplayName, f => f.Name.FullName());
        RuleFor(p => p.SalesforceAccountId, f => "001" + f.Random.AlphaNumeric(15).ToUpperInvariant());
        RuleFor(p => p.LastSyncedAtUtc, f => f.Date.Recent(30).ToUniversalTime());
    }

    public static List<PartyPersonCache> GeneratePersonDetails(List<PartyCache> parties, Faker faker)
    {
        return parties.Select(party => new PartyPersonCache
        {
            PartyId = party.Id,
            FirstName = faker.Name.FirstName(),
            MiddleName = faker.Random.Bool(0.3f) ? faker.Name.FirstName() : null,
            LastName = faker.Name.LastName(),
            Email = faker.Internet.Email(),
            Phone = faker.Phone.PhoneNumber("###-###-####"),
            CoBuyerFirstName = faker.Random.Bool(0.4f) ? faker.Name.FirstName() : null,
            CoBuyerLastName = faker.Random.Bool(0.4f) ? faker.Name.LastName() : null,
            PrimarySalesPersonFederatedId = faker.Random.Uuid().ToString(),
            PrimarySalesPersonFirstName = faker.Name.FirstName(),
            PrimarySalesPersonLastName = faker.Name.LastName(),
            SecondarySalesPersonFederatedId = faker.Random.Bool(0.3f) ? faker.Random.Uuid().ToString() : null,
            SecondarySalesPersonFirstName = faker.Random.Bool(0.3f) ? faker.Name.FirstName() : null,
            SecondarySalesPersonLastName = faker.Random.Bool(0.3f) ? faker.Name.LastName() : null
        }).ToList();
    }
}
