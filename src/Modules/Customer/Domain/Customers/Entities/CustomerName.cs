using Rtl.Core.Domain.Auditing;
using Rtl.Core.Domain.ValueObjects;

namespace Modules.Customer.Domain.Customers.Entities;

// Nullable fields — Leads from Vantage may have incomplete name info.
public sealed class CustomerName : ValueObject
{
    private CustomerName() {}

    [SensitiveData] public string? FirstName { get; private set; }
    [SensitiveData] public string? MiddleName { get; private set; }
    [SensitiveData] public string? LastName { get; private set; }
    public string? NameExtension { get; private set; }

    public static CustomerName Create(
        string? firstName,
        string? middleName,
        string? lastName,
        string? nameExtension = null)
    {
        return new CustomerName
        {
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            NameExtension = nameExtension
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FirstName;
        yield return MiddleName;
        yield return LastName;
        yield return NameExtension;
    }
}
