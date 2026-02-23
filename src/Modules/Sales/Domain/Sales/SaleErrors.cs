using Rtl.Core.Domain.Results;

namespace Modules.Sales.Domain.Sales;

public static class SaleErrors
{
    public static Error NotFound(int saleId) =>
        Error.NotFound("Sale.NotFound", $"Sale with ID '{saleId}' was not found.");

    public static Error NotFoundByPublicId(Guid publicId) =>
        Error.NotFound("Sale.NotFound", $"Sale with PublicId '{publicId}' was not found.");

    public static Error PartyNotFound(Guid publicId) =>
        Error.NotFound("Party.NotFound", $"Party with PublicId '{publicId}' was not found in cache.");

    public static Error RetailLocationNotFound(int homeCenterNumber) =>
        Error.NotFound("RetailLocation.NotFound", $"Retail location with HomeCenterNumber '{homeCenterNumber}' was not found.");

    public static Error RetailLocationInactive(int homeCenterNumber) =>
        Error.Problem("RetailLocation.Inactive", $"Retail location with HomeCenterNumber '{homeCenterNumber}' is not active.");

    public static Error DuplicateSaleForParty(Guid partyPublicId) =>
        Error.Conflict("Sale.DuplicateForParty", $"A sale already exists for party '{partyPublicId}'.");
}
