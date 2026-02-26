using Modules.Sales.Domain.Packages.Home;
using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.Packages.UpdatePackageHome;

public sealed record UpdatePackageHomeCommand(
    Guid PackagePublicId,
    UpdatePackageHomeRequest Home) : ICommand<UpdatePackageHomeResult>;

public sealed record UpdatePackageHomeResult(
    decimal GrossProfit,
    decimal CommissionableGrossProfit,
    bool MustRecalculateTaxes);

public sealed record UpdatePackageHomeRequest(
    // Pricing (maps to PackageLine columns)
    decimal SalePrice,
    decimal EstimatedCost,
    decimal RetailSalePrice,

    // Identification
    string? StockNumber,
    HomeType HomeType,
    HomeSourceType HomeSourceType,
    ModularType? ModularType,

    // Physical specs
    string? Vendor,
    string? Make,
    string? Model,
    int? ModelYear,
    decimal? LengthInFeet,
    decimal? WidthInFeet,
    int? Bedrooms,
    decimal? Bathrooms,
    string? SquareFootage,
    string[]? SerialNumbers,

    // Cost breakdown
    decimal? BaseCost,
    decimal? OptionsCost,
    decimal? FreightCost,
    decimal? InvoiceCost,
    decimal? NetInvoice,
    decimal? GrossCost,
    decimal? TaxIncludedOnInvoice,

    // Transport
    int? NumberOfWheels,
    int? NumberOfAxles,
    WheelAndAxlesOption? WheelAndAxlesOption, // null = "none"
    int? NumberOfFloorSections,
    decimal? CarrierFrameDeposit,
    decimal? RebateOnMfgInvoice,

    // Classification
    bool? ClaytonBuilt,
    string? BuildType,
    string? InventoryReferenceId,

    // Fees
    decimal? StateAssociationAndMhiDues,
    decimal? PartnerAssistance,
    double? DistanceMiles,

    // Property/listing
    string? PropertyType,
    string? PurchaseOption,
    decimal? ListingPrice,
    string? AccountNumber,
    string? DisplayAccountId,

    // Address (used/repo homes)
    string? StreetAddress,
    string? City,
    string? State,
    string? ZipCode);
