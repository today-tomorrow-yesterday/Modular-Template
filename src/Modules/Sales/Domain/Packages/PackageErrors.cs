using Rtl.Core.Domain.Results;

namespace Modules.Sales.Domain.Packages;

public static class PackageErrors
{
    public static Error NotFound(int packageId) =>
        Error.NotFound("Package.NotFound", $"Package with ID '{packageId}' was not found.");

    public static Error NotFoundByPublicId(Guid publicId) =>
        Error.NotFound("Package.NotFound", $"Package with PublicId '{publicId}' was not found.");

    public static Error DuplicateName(string name) =>
        Error.Conflict("Package.DuplicateName", $"A package with name '{name}' already exists for this sale.");

    public static Error InvalidAuthorizedUsers() =>
        Error.Problem("Package.InvalidAuthorizedUsers", "One or more authorized user IDs are invalid, inactive, or retired.");

    public static Error NoPrimaryPackage() =>
        Error.NotFound("Package.NoPrimaryPackage", "No primary package exists for this sale.");

    public static Error CannotDeletePrimary() =>
        Error.Conflict("Package.CannotDeletePrimary", "Cannot delete the primary package while other packages exist. Reassign primary first.");

    public static Error CannotDeleteLastPackage() =>
        Error.Conflict("Package.CannotDeleteLastPackage", "Cannot delete the sole remaining package on a sale.");

    public static Error OnlyDraftCanBeDeleted() =>
        Error.Conflict("Package.OnlyDraftCanBeDeleted", "Only packages in Draft status can be deleted.");
}
