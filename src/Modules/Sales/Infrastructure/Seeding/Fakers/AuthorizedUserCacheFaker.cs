using Bogus;
using Modules.Sales.Domain.AuthorizedUsersCache;

namespace Modules.Sales.Infrastructure.Seeding.Fakers;

internal sealed class AuthorizedUserCacheFaker : Faker<AuthorizedUserCache>
{
    private int _id;
    private int _refUserIndex;
    private int _employeeNumber;
    private int _hcIndex;

    public AuthorizedUserCacheFaker(int[] homeCenterNumbers)
    {
        _id = 0;
        _refUserIndex = 0;
        _employeeNumber = 1000;
        _hcIndex = 0;

        RuleFor(u => u.Id, _ => ++_id);
        RuleFor(u => u.RefUserId, f => SeedConstants.DeterministicGuid("authorized-user", ++_refUserIndex));
        RuleFor(u => u.FederatedId, f => f.Random.Uuid().ToString());
        RuleFor(u => u.EmployeeNumber, _ => ++_employeeNumber);
        RuleFor(u => u.FirstName, f => f.Name.FirstName());
        RuleFor(u => u.LastName, f => f.Name.LastName());
        RuleFor(u => u.DisplayName, (f, u) => $"{u.FirstName} {u.LastName}");
        RuleFor(u => u.EmailAddress, (f, u) => f.Internet.Email(u.FirstName, u.LastName));
        RuleFor(u => u.IsActive, true);
        RuleFor(u => u.IsRetired, false);
        // Each user is authorized for their primary home center (round-robin) plus the next one.
        // Deterministic regardless of Bogus seed — User 1 → [100,200], User 2 → [200,300], ...
        RuleFor(u => u.AuthorizedHomeCenters, _ =>
        {
            var primary = homeCenterNumbers[_hcIndex % homeCenterNumbers.Length];
            var secondary = homeCenterNumbers[(_hcIndex + 1) % homeCenterNumbers.Length];
            _hcIndex++;
            return new[] { primary, secondary };
        });
        RuleFor(u => u.LastSyncedAtUtc, f => f.Date.Recent(30).ToUniversalTime());
    }
}
