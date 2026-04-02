using ModularTemplate.Domain.Results;

namespace Modules.SampleOrders.Domain.Customers;

public static class CustomerErrors
{
    public static Error NotFound(Guid publicId) =>
        Error.NotFound("Customers.NotFound", $"The customer with ID '{publicId}' was not found.");

    public static readonly Error NotFoundByPublicId =
        Error.NotFound("Customers.NotFoundByPublicId", "Customer not found by PublicId.");

    public static readonly Error NameEmpty =
        Error.Validation("Customers.NameEmpty", "The customer name cannot be empty.");

    public static readonly Error EmailEmpty =
        Error.Validation("Customers.EmailEmpty", "The customer email cannot be empty.");

    public static readonly Error EmailInvalid =
        Error.Validation("Customers.EmailInvalid", "The customer email format is invalid.");
}
