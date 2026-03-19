using Modules.Customer.Domain.Customers.Enums;
using Rtl.Core.Domain.Results;

namespace Modules.Customer.Domain.Customers.Errors;

public static class CustomerErrors
{
    public static Error NotFound(int customerId) =>
        Error.NotFound("Customers.NotFound", $"The customer with ID '{customerId}' was not found.");

    public static Error NotFoundByPublicId(Guid publicId) =>
        Error.NotFound("Customers.NotFoundByPublicId", $"The customer with public ID '{publicId}' was not found.");

    public static Error NotFoundByIdentifier(IdentifierType type, string value) =>
        Error.NotFound("Customers.NotFoundByIdentifier", $"No customer found with {type} '{value}'.");

    public static readonly Error InvalidLifecycleTransition =
        Error.Validation("Customers.InvalidLifecycleTransition", "The requested lifecycle transition is not valid.");

    public static readonly Error VmfLosUnavailable =
        Error.Failure("Customers.VmfLosUnavailable", "VMF LOS service is unavailable. The request will be retried.");

    public static readonly Error BorrowerNotFound =
        Error.NotFound("Customers.BorrowerNotFound", "No borrower data found in VMF LOS for the specified loan.");
}
