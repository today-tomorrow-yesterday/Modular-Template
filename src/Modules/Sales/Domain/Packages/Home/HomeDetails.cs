using Rtl.Core.Domain.Entities;
using System.Text.Json;

namespace Modules.Sales.Domain.Packages.Home;

public enum HomeType
{
    New,
    Used,
    Repo
}

public enum HomeSourceType
{
    OnLot,
    Quoted,
    VmfHomes,
    Manual
}

public enum ModularType
{
    Hud,
    OnFrame,
    Mod,
    OffFrame
}

public enum WheelAndAxlesOption
{
    Rent,
    Purchase
}

public sealed class HomeDetails : IVersionedDetails
{
    private HomeDetails() { }

    public int SchemaVersion => 1;
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }

    // Stock & Identification
    public string? InventoryReferenceId { get; private set; }
    public string[]? SerialNumbers { get; private set; }
    public string? StockNumber { get; private set; } // tax-affecting field
    public HomeType HomeType { get; private set; } // tax-affecting field
    public HomeSourceType HomeSourceType { get; private set; }

    // Dimensions & Classification
    public ModularType? ModularType { get; private set; }
    public string? Model { get; private set; }
    public string? Make { get; private set; }
    public int? ModelYear { get; private set; }
    public decimal? LengthInFeet { get; private set; }
    public decimal? WidthInFeet { get; private set; }
    public int? Bedrooms { get; private set; }
    public decimal? Bathrooms { get; private set; }
    public string? SquareFootage { get; private set; }
    public int? NumberOfFloorSections { get; private set; }
    public string? BuildType { get; private set; }
    public bool? ClaytonBuilt { get; private set; }

    // Costs — iSeries-sourced
    public decimal? BaseCost { get; private set; }
    public decimal? OptionsCost { get; private set; }
    public decimal? FreightCost { get; private set; }
    public decimal? InvoiceCost { get; private set; }
    public decimal? NetInvoice { get; private set; }
    public decimal? GrossCost { get; private set; }
    public decimal? TaxIncludedOnInvoice { get; private set; }
    public decimal? StateAssociationAndMhiDues { get; private set; }
    public decimal? RebateOnMfgInvoice { get; private set; }
    public decimal? PartnerAssistance { get; private set; }

    // Transport — W&A cost calculation inputs
    public int? NumberOfWheels { get; private set; }
    public int? NumberOfAxles { get; private set; }
    public WheelAndAxlesOption? WheelAndAxlesOption { get; private set; }
    public decimal? CarrierFrameDeposit { get; private set; }
    public double? DistanceMiles { get; private set; }

    // Property & Listing
    public string? PropertyType { get; private set; }
    public string? PurchaseOption { get; private set; }
    public string? Vendor { get; private set; }
    public string? AccountNumber { get; private set; }
    public string? DisplayAccountId { get; private set; }
    public decimal? ListingPrice { get; private set; }

    // Home address (for used/repo homes)
    public string? StreetAddress { get; private set; }
    public string? City { get; private set; }
    public string? State { get; private set; }
    public string? ZipCode { get; private set; }

    public static HomeDetails Create(
        HomeType homeType,
        HomeSourceType homeSourceType,
        string? stockNumber = null,
        ModularType? modularType = null,
        string? vendor = null,
        string? make = null,
        string? model = null,
        int? modelYear = null,
        decimal? lengthInFeet = null,
        decimal? widthInFeet = null,
        int? bedrooms = null,
        decimal? bathrooms = null,
        string? squareFootage = null,
        string[]? serialNumbers = null,
        decimal? baseCost = null,
        decimal? optionsCost = null,
        decimal? freightCost = null,
        decimal? invoiceCost = null,
        decimal? netInvoice = null,
        decimal? grossCost = null,
        decimal? taxIncludedOnInvoice = null,
        int? numberOfWheels = null,
        int? numberOfAxles = null,
        WheelAndAxlesOption? wheelAndAxlesOption = null,
        int? numberOfFloorSections = null,
        decimal? carrierFrameDeposit = null,
        decimal? rebateOnMfgInvoice = null,
        bool? claytonBuilt = null,
        string? buildType = null,
        string? inventoryReferenceId = null,
        decimal? stateAssociationAndMhiDues = null,
        decimal? partnerAssistance = null,
        double? distanceMiles = null,
        string? propertyType = null,
        string? purchaseOption = null,
        decimal? listingPrice = null,
        string? accountNumber = null,
        string? displayAccountId = null,
        string? streetAddress = null,
        string? city = null,
        string? state = null,
        string? zipCode = null)
    {
        return new HomeDetails
        {
            HomeType = homeType,
            HomeSourceType = homeSourceType,
            StockNumber = stockNumber,
            ModularType = modularType,
            Vendor = vendor,
            Make = make,
            Model = model,
            ModelYear = modelYear,
            LengthInFeet = lengthInFeet,
            WidthInFeet = widthInFeet,
            Bedrooms = bedrooms,
            Bathrooms = bathrooms,
            SquareFootage = squareFootage,
            SerialNumbers = serialNumbers?.ToArray(),
            BaseCost = baseCost,
            OptionsCost = optionsCost,
            FreightCost = freightCost,
            InvoiceCost = invoiceCost,
            NetInvoice = netInvoice,
            GrossCost = grossCost,
            TaxIncludedOnInvoice = taxIncludedOnInvoice,
            NumberOfWheels = numberOfWheels,
            NumberOfAxles = numberOfAxles,
            WheelAndAxlesOption = wheelAndAxlesOption,
            NumberOfFloorSections = numberOfFloorSections,
            CarrierFrameDeposit = carrierFrameDeposit,
            RebateOnMfgInvoice = rebateOnMfgInvoice,
            ClaytonBuilt = claytonBuilt,
            BuildType = buildType,
            InventoryReferenceId = inventoryReferenceId,
            StateAssociationAndMhiDues = stateAssociationAndMhiDues,
            PartnerAssistance = partnerAssistance,
            DistanceMiles = distanceMiles,
            PropertyType = propertyType,
            PurchaseOption = purchaseOption,
            ListingPrice = listingPrice,
            AccountNumber = accountNumber,
            DisplayAccountId = displayAccountId,
            StreetAddress = streetAddress,
            City = city,
            State = state,
            ZipCode = zipCode
        };
    }
}
