using Modules.SampleOrders.Domain.Customers.Events;
using Modules.SampleOrders.Domain.ValueObjects;
using ModularTemplate.Domain.Auditing;
using ModularTemplate.Domain.Entities;
using ModularTemplate.Domain.Results;

namespace Modules.SampleOrders.Domain.Customers;

public sealed class Customer : SoftDeletableEntity, IAggregateRoot
{
    private readonly List<CustomerContact> _contacts = [];
    private readonly List<CustomerAddress> _addresses = [];

    private Customer() { }

    public Guid PublicId { get; private set; }

    public CustomerName Name { get; private set; } = null!;

    public CustomerStatus Status { get; private set; }

    [SensitiveData]
    public DateOnly? DateOfBirth { get; private set; }

    public IReadOnlyCollection<CustomerContact> Contacts => _contacts.AsReadOnly();
    public IReadOnlyCollection<CustomerAddress> Addresses => _addresses.AsReadOnly();

    // ─── Factory Methods ───────────────────────────────────────────

    public static Result<Customer> Create(
        string firstName,
        string? middleName,
        string lastName,
        string? email,
        DateOnly? dateOfBirth = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            return Result.Failure<Customer>(CustomerErrors.NameEmpty);
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            return Result.Failure<Customer>(CustomerErrors.NameEmpty);
        }

        var customer = new Customer
        {
            PublicId = Guid.CreateVersion7(),
            Name = CustomerName.Create(firstName, middleName, lastName),
            Status = CustomerStatus.Active,
            DateOfBirth = dateOfBirth
        };

        if (!string.IsNullOrWhiteSpace(email))
        {
            customer._contacts.Add(
                CustomerContact.Create(customer.Id, ContactType.Email, email, isPrimary: true));
        }

        customer.Raise(new CustomerCreatedDomainEvent());

        return customer;
    }

    // ─── Behavioral Methods ────────────────────────────────────────

    public Result UpdateName(string firstName, string? middleName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            return Result.Failure(CustomerErrors.NameEmpty);
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            return Result.Failure(CustomerErrors.NameEmpty);
        }

        var newName = CustomerName.Create(firstName, middleName, lastName);
        if (Name == newName)
        {
            return Result.Success();
        }

        Name = newName;
        Raise(new CustomerUpdatedDomainEvent());

        return Result.Success();
    }

    public Result UpdateStatus(CustomerStatus newStatus)
    {
        if (Status == newStatus)
        {
            return Result.Success();
        }

        Status = newStatus;
        Raise(new CustomerUpdatedDomainEvent());

        return Result.Success();
    }

    public void AddContact(ContactType type, string value, bool isPrimary = false)
    {
        if (isPrimary)
        {
            foreach (var existing in _contacts.Where(c => c.Type == type && c.IsPrimary))
            {
                existing.SetPrimary(false);
            }
        }

        var match = _contacts.FirstOrDefault(c => c.Type == type && c.Value == value);
        if (match is not null)
        {
            match.SetPrimary(isPrimary);
            return;
        }

        _contacts.Add(CustomerContact.Create(Id, type, value, isPrimary));
        Raise(new CustomerContactsChangedDomainEvent());
    }

    public Result AddAddress(Address address, bool isPrimary = false)
    {
        _addresses.Add(CustomerAddress.Create(Id, address, isPrimary));
        Raise(new CustomerAddressChangedDomainEvent());

        return Result.Success();
    }

    public string? GetPrimaryEmail() =>
        _contacts.FirstOrDefault(c => c.Type == ContactType.Email && c.IsPrimary)?.Value;

    public string? GetPrimaryPhone() =>
        _contacts.FirstOrDefault(c => c.Type == ContactType.Phone && c.IsPrimary)?.Value;
}
