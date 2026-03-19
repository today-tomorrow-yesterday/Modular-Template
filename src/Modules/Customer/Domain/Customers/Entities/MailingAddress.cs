using Rtl.Core.Domain.Auditing;
using Rtl.Core.Domain.ValueObjects;

namespace Modules.Customer.Domain.Customers.Entities;

// Mailing address from Salesforce/CRM. Owned entity, flattened onto the customers table.
// Delivery address lives in Sales module (sales.delivery_addresses) — separate concern.
public sealed class MailingAddress : ValueObject
{
    private MailingAddress(){}

    [SensitiveData] public string? AddressLine1 { get; private set; }
    [SensitiveData] public string? AddressLine2 { get; private set; }
    public string? City { get; private set; }
    public string? County { get; private set; }
    public string? State { get; private set; }
    public string? Country { get; private set; }
    public string? PostalCode { get; private set; }

    public static MailingAddress Create(
        string? addressLine1,
        string? addressLine2,
        string? city,
        string? county,
        string? state,
        string? country,
        string? postalCode)
    {
        return new MailingAddress
        {
            AddressLine1 = addressLine1,
            AddressLine2 = addressLine2,
            City = city,
            County = county,
            State = state,
            Country = country,
            PostalCode = postalCode
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return AddressLine1;
        yield return AddressLine2;
        yield return City;
        yield return County;
        yield return State;
        yield return Country;
        yield return PostalCode;
    }
}
