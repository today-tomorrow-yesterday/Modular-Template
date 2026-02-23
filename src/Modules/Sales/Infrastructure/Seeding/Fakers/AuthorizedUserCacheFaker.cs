using Bogus;
using Modules.Sales.Domain.AuthorizedUsersCache;

namespace Modules.Sales.Infrastructure.Seeding.Fakers;

internal sealed class AuthorizedUserCacheFaker : Faker<AuthorizedUserCache>
{
    private int _id;
    private int _refUserId;
    private int _employeeNumber;

    public AuthorizedUserCacheFaker(int[] homeCenterNumbers)
    {
        _id = 0;
        _refUserId = 0;
        _employeeNumber = 1000;

        RuleFor(u => u.Id, _ => ++_id);
        RuleFor(u => u.RefUserId, _ => ++_refUserId);
        RuleFor(u => u.FederatedId, f => f.Random.Uuid().ToString());
        RuleFor(u => u.EmployeeNumber, _ => ++_employeeNumber);
        RuleFor(u => u.FirstName, f => f.Name.FirstName());
        RuleFor(u => u.LastName, f => f.Name.LastName());
        RuleFor(u => u.DisplayName, (f, u) => $"{u.FirstName} {u.LastName}");
        RuleFor(u => u.EmailAddress, (f, u) => f.Internet.Email(u.FirstName, u.LastName));
        RuleFor(u => u.IsActive, true);
        RuleFor(u => u.IsRetired, false);
        RuleFor(u => u.AuthorizedHomeCenters, f => f.PickRandom(homeCenterNumbers, f.Random.Int(1, 3)).ToArray());
        RuleFor(u => u.LastSyncedAtUtc, f => f.Date.Recent(30).ToUniversalTime());
    }
}
