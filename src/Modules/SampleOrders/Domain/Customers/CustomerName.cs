namespace Modules.SampleOrders.Domain.Customers;

public sealed record CustomerName
{
    private CustomerName() { }

    public string FirstName { get; private init; } = string.Empty;
    public string? MiddleName { get; private init; }
    public string LastName { get; private init; } = string.Empty;

    public string FullName => string.IsNullOrWhiteSpace(MiddleName)
        ? $"{FirstName} {LastName}".Trim()
        : $"{FirstName} {MiddleName} {LastName}".Trim();

    public static CustomerName Create(string firstName, string? middleName, string lastName)
    {
        return new CustomerName
        {
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName
        };
    }
}
