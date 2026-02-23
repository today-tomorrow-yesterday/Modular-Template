using Rtl.Core.Application.Messaging;

namespace Modules.Inventory.Application.RepoInventory.GetRepoInventory;

public sealed record GetRepoInventoryQuery(
    double? Latitude,
    double? Longitude,
    double? MaxDistance,
    int? AccountId) : IQuery<IReadOnlyCollection<RepoInventoryResponse>>;

public sealed record RepoInventoryResponse(
    int HomeCenterNumber,
    string? PropertyType,
    string? PurchaseOption,
    string? BuildType,
    decimal? Width,
    decimal? Length,
    int? Bedrooms,
    int? Bathrooms,
    int? ModelYear,
    string? Model,
    string? Facility,
    string? SerialNumber,
    string? StreetAddress,
    string? City,
    string? State,
    string? ZipCode,
    string? AccountNumber,
    decimal? TotalInvoiceAmount,
    string? HomeInventorySource,
    string? HomeInventoryType);
