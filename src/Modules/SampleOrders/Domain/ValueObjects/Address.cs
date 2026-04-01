namespace Modules.SampleOrders.Domain.ValueObjects;

/// <summary>
/// Value object representing a physical address.
/// Used by both CustomerAddress and ShippingAddress entities.
/// Equality is by value — two Address instances with the same fields are equal.
/// </summary>
public sealed record Address
{
    private Address() { }

    public string? AddressLine1 { get; private init; }
    public string? AddressLine2 { get; private init; }
    public string? City { get; private init; }
    public string? State { get; private init; }
    public string? PostalCode { get; private init; }
    public string? Country { get; private init; }

    public static Address Create(
        string? addressLine1,
        string? addressLine2,
        string? city,
        string? state,
        string? postalCode,
        string? country = "US")
    {
        return new Address
        {
            AddressLine1 = addressLine1,
            AddressLine2 = addressLine2,
            City = city,
            State = state,
            PostalCode = postalCode,
            Country = country
        };
    }
}
