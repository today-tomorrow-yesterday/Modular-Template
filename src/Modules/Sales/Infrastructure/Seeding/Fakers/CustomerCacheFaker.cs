using Bogus;
using Modules.Sales.Domain.CustomersCache;

namespace Modules.Sales.Infrastructure.Seeding.Fakers;

internal sealed class CustomerCacheFaker : Faker<CustomerCache>
{
    private int _customerIndex;
    private int _hcIndex;

    public CustomerCacheFaker(int[] homeCenterNumbers)
    {
        _customerIndex = 0;
        _hcIndex = 0;

        RuleFor(c => c.RefPublicId, _ => SeedConstants.DeterministicGuid("customer", ++_customerIndex));
        RuleFor(c => c.LifecycleStage, f => f.PickRandom<LifecycleStage>());
        // Round-robin home center assignment — deterministic regardless of Bogus seed.
        RuleFor(c => c.HomeCenterNumber, _ => homeCenterNumbers[_hcIndex++ % homeCenterNumbers.Length]);
        RuleFor(c => c.DisplayName, f => f.Name.FullName());
        RuleFor(c => c.SalesforceAccountId, f => "001" + f.Random.AlphaNumeric(15).ToUpperInvariant());
        RuleFor(c => c.LastSyncedAtUtc, f => f.Date.Recent(30).ToUniversalTime());
        RuleFor(c => c.FirstName, f => f.Name.FirstName());
        RuleFor(c => c.MiddleName, f => f.Random.Bool(0.3f) ? f.Name.FirstName() : null);
        RuleFor(c => c.LastName, f => f.Name.LastName());
        RuleFor(c => c.Email, f => f.Internet.Email());
        RuleFor(c => c.Phone, f => f.Phone.PhoneNumber("###-###-####"));
        RuleFor(c => c.CoBuyerFirstName, f => f.Random.Bool(0.4f) ? f.Name.FirstName() : null);
        RuleFor(c => c.CoBuyerLastName, f => f.Random.Bool(0.4f) ? f.Name.LastName() : null);
        RuleFor(c => c.PrimarySalesPersonFederatedId, f => f.Random.Uuid().ToString());
        RuleFor(c => c.PrimarySalesPersonFirstName, f => f.Name.FirstName());
        RuleFor(c => c.PrimarySalesPersonLastName, f => f.Name.LastName());
        RuleFor(c => c.SecondarySalesPersonFederatedId, f => f.Random.Bool(0.3f) ? f.Random.Uuid().ToString() : null);
        RuleFor(c => c.SecondarySalesPersonFirstName, f => f.Random.Bool(0.3f) ? f.Name.FirstName() : null);
        RuleFor(c => c.SecondarySalesPersonLastName, f => f.Random.Bool(0.3f) ? f.Name.LastName() : null);
    }
}
