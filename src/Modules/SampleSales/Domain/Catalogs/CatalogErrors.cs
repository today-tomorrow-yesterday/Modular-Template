using Rtl.Core.Domain.Results;

namespace Modules.SampleSales.Domain.Catalogs;

public static class CatalogErrors
{
    public static Error NotFound(int catalogId) =>
        Error.NotFound("Catalogs.NotFound", $"The catalog with ID '{catalogId}' was not found.");

    public static readonly Error NameEmpty =
        Error.Validation("Catalogs.NameEmpty", "The catalog name cannot be empty.");

    public static readonly Error NameTooLong =
        Error.Validation("Catalogs.NameTooLong", "The catalog name cannot exceed 200 characters.");

    public static readonly Error ProductAlreadyInCatalog =
        Error.Conflict("Catalogs.ProductAlreadyInCatalog", "The product is already in the catalog.");

    public static readonly Error ProductNotInCatalog =
        Error.NotFound("Catalogs.ProductNotInCatalog", "The product is not in the catalog.");
}
