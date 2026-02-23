using Modules.SampleOrders.Domain.Customers.Events;
using Modules.SampleOrders.Domain.ValueObjects;
using Rtl.Core.Domain.Entities;
using Rtl.Core.Domain.Results;

namespace Modules.SampleOrders.Domain.Customers;

public sealed class Customer : SoftDeletableEntity, IAggregateRoot
{
    private const int MaxNameLength = 200;

    private Customer() {}

    public string Name { get; private set; } = string.Empty;

    public Email Email { get; private set; } = null!;

    public static Result<Customer> Create(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Customer>(CustomerErrors.NameEmpty);
        }

        if (name.Length > MaxNameLength)
        {
            return Result.Failure<Customer>(CustomerErrors.NameTooLong);
        }

        var emailResult = ValueObjects.Email.Create(email);
        if (emailResult.IsFailure)
        {
            return Result.Failure<Customer>(emailResult.Error);
        }

        var customer = new Customer
        {
            Name = name.Trim(),
            Email = emailResult.Value
        };

        customer.Raise(new CustomerCreatedDomainEvent());

        return customer;
    }

    public Result Update(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(CustomerErrors.NameEmpty);
        }

        if (name.Length > MaxNameLength)
        {
            return Result.Failure(CustomerErrors.NameTooLong);
        }

        var emailResult = ValueObjects.Email.Create(email);
        if (emailResult.IsFailure)
        {
            return Result.Failure(emailResult.Error);
        }

        Name = name.Trim();
        Email = emailResult.Value;

        Raise(new CustomerUpdatedDomainEvent());

        return Result.Success();
    }
}
