using Rtl.Core.Domain.Results;

namespace Modules.Sales.Domain.Packages;

public static class WarrantyErrors
{
    public static Error NoDeliveryAddress() =>
        Error.Problem("Warranty.NoDeliveryAddress", "A delivery address is required to generate a warranty quote.");

    public static Error NoHomeLine() =>
        Error.Problem("Warranty.NoHomeLine", "A home line is required on the primary package to generate a warranty quote.");

    public static Error NoPrimaryPackage() =>
        Error.Problem("Warranty.NoPrimaryPackage", "A primary package is required to generate a warranty quote.");

    public static Error MissingHomeDetails() =>
        Error.Problem("Warranty.MissingHomeDetails", "Home details (ModelYear, ModularType) are required to generate a warranty quote.");

    public static Error IneligibleOccupancy(string occupancyType) =>
        Error.Problem("Warranty.IneligibleOccupancy", $"Occupancy type '{occupancyType}' is not eligible for a warranty quote.");
}
