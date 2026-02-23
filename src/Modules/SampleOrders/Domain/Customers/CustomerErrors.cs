using Rtl.Core.Domain.Results;

namespace Modules.SampleOrders.Domain.Customers;

public static class CustomerErrors
{
    public static Error NotFound(int customerId) =>
        Error.NotFound("Customers.NotFound", $"The customer with ID '{customerId}' was not found.");

    public static readonly Error NameEmpty =
        Error.Validation("Customers.NameEmpty", "The customer name cannot be empty.");

    public static readonly Error NameTooLong =
        Error.Validation("Customers.NameTooLong", "The customer name cannot exceed 200 characters.");

    public static readonly Error EmailEmpty =
        Error.Validation("Customers.EmailEmpty", "The customer email cannot be empty.");

    public static readonly Error EmailInvalid =
        Error.Validation("Customers.EmailInvalid", "The customer email format is invalid.");
}
