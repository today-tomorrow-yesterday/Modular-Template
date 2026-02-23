using Modules.Customer.Domain.Parties.Enums;
using Rtl.Core.Domain.Results;

namespace Modules.Customer.Domain.Parties.Errors;

public static class PartyErrors
{
    public static Error NotFound(int partyId) =>
        Error.NotFound("Parties.NotFound", $"The party with ID '{partyId}' was not found.");

    public static Error NotFoundByPublicId(Guid publicId) =>
        Error.NotFound("Parties.NotFoundByPublicId", $"The party with public ID '{publicId}' was not found.");

    public static Error NotFoundByIdentifier(IdentifierType type, string value) =>
        Error.NotFound("Parties.NotFoundByIdentifier", $"No party found with {type} '{value}'.");

    public static Error NotFoundByCrmCustomerId(int customerId) =>
        Error.NotFound("Parties.NotFoundByCrmCustomerId", $"No party found with CRM customer ID '{customerId}'.");

    public static readonly Error InvalidLifecycleTransition =
        Error.Validation("Parties.InvalidLifecycleTransition", "The requested lifecycle transition is not valid.");

    public static readonly Error PersonNameRequiredForPerson =
        Error.Validation("Parties.PersonNameRequired", "Person parties must have a name.");

    public static readonly Error OrganizationNameRequiredForOrg =
        Error.Validation("Parties.OrganizationNameRequired", "Organization parties must have an organization name.");

    public static Error InvalidPartyTypeData(PartyType partyType) =>
        Error.Validation("Parties.InvalidPartyTypeData", $"Invalid PartyType '{partyType}' or missing type-specific data.");

    public static readonly Error VmfLosUnavailable =
        Error.Failure("Parties.VmfLosUnavailable", "VMF LOS service is unavailable. The request will be retried.");

    public static readonly Error BorrowerNotFound =
        Error.NotFound("Parties.BorrowerNotFound", "No borrower data found in VMF LOS for the specified loan.");
}
