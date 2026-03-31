using Rtl.Core.Domain.Results;

namespace Modules.SampleOrders.Domain.Customers;

public static class CustomerErrors
{
    public static readonly Error NotFound =
        Error.NotFound("Customers.NotFound", "Customer not found.");

    public static readonly Error NotFoundByPublicId =
        Error.NotFound("Customers.NotFoundByPublicId", "Customer not found by PublicId.");

    public static readonly Error NameEmpty =
        Error.Validation("Customers.NameEmpty", "The customer name cannot be empty.");

    public static readonly Error EmailEmpty =
        Error.Validation("Customers.EmailEmpty", "The customer email cannot be empty.");

    public static readonly Error EmailInvalid =
        Error.Validation("Customers.EmailInvalid", "The customer email format is invalid.");
}
