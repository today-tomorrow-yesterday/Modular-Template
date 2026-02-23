using Rtl.Core.Domain.Auditing;

namespace Modules.Customer.Domain.SalesPersons;

// Sales associate reference data from Salesforce CDC.
// Not independently managed — upserted during Party CDC sync processing.
public sealed class SalesPerson
{
    private SalesPerson() {}

    public string Id { get; private set; } = null!;

    [SensitiveData] public string Email { get; private set; } = null!;
    [SensitiveData] public string Username { get; private set; } = null!;
    [SensitiveData] public string FirstName { get; private set; } = null!;
    [SensitiveData] public string LastName { get; private set; } = null!;
    public int? LotNumber { get; private set; }
    [SensitiveData] public string FederatedId { get; private set; } = null!;

    public DateTime LastSyncedAtUtc { get; private set; }

    public static SalesPerson Assign(
        string id,
        string email,
        string username,
        string firstName,
        string lastName,
        int? lotNumber,
        string federatedId)
    {
        return new SalesPerson
        {
            Id = id,
            Email = email,
            Username = username,
            FirstName = firstName,
            LastName = lastName,
            LotNumber = lotNumber,
            FederatedId = federatedId,
            LastSyncedAtUtc = DateTime.UtcNow
        };
    }

    public void Update(
        string email,
        string username,
        string firstName,
        string lastName,
        int? lotNumber,
        string federatedId)
    {
        Email = email;
        Username = username;
        FirstName = firstName;
        LastName = lastName;
        LotNumber = lotNumber;
        FederatedId = federatedId;
        LastSyncedAtUtc = DateTime.UtcNow;
    }
}
