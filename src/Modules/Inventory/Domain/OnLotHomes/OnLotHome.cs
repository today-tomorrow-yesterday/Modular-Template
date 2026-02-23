using Modules.Inventory.Domain.OnLotHomes.Events;
using Rtl.Core.Domain.Auditing;
using Rtl.Core.Domain.Entities;

namespace Modules.Inventory.Domain.OnLotHomes;

public sealed class OnLotHome : Entity, IAggregateRoot
{
    private OnLotHome() { }

    public int RefHomeCenterNumber { get; private set; }

    public string RefStockNumber { get; private set; } = string.Empty;

    public string? StockType { get; private set; }

    public string? Condition { get; private set; }

    public string? BuildType { get; private set; }

    public decimal? Width { get; private set; }

    public decimal? Length { get; private set; }

    public int? NumberOfBedrooms { get; private set; }

    public int? NumberOfBathrooms { get; private set; }

    public int? ModelYear { get; private set; }

    public string? Model { get; private set; }

    public string? Make { get; private set; }

    public string? Facility { get; private set; }

    [SensitiveData] public string? SerialNumber { get; private set; }

    [SensitiveData] public decimal? TotalInvoiceAmount { get; private set; }

    [SensitiveData] public decimal? PurchaseDiscount { get; private set; }

    [SensitiveData] public decimal? OriginalRetailPrice { get; private set; }

    [SensitiveData] public decimal? CurrentRetailPrice { get; private set; }

    public string? StockedInDate { get; private set; }

    public string? LandStockNumber { get; private set; }

    public DateTime LastSyncedAtUtc { get; private set; }

    public static OnLotHome Create(
        int id,
        int refHomeCenterNumber,
        string refStockNumber,
        string? stockType,
        string? condition,
        string? buildType,
        decimal? width,
        decimal? length,
        int? numberOfBedrooms,
        int? numberOfBathrooms,
        int? modelYear,
        string? model,
        string? make,
        string? facility,
        string? serialNumber,
        decimal? totalInvoiceAmount,
        decimal? purchaseDiscount,
        decimal? originalRetailPrice,
        decimal? currentRetailPrice,
        string? stockedInDate,
        string? landStockNumber,
        DateTime lastSyncedAtUtc)
    {
        var home = new OnLotHome
        {
            Id = id,
            RefHomeCenterNumber = refHomeCenterNumber,
            RefStockNumber = refStockNumber,
            StockType = stockType,
            Condition = condition,
            BuildType = buildType,
            Width = width,
            Length = length,
            NumberOfBedrooms = numberOfBedrooms,
            NumberOfBathrooms = numberOfBathrooms,
            ModelYear = modelYear,
            Model = model,
            Make = make,
            Facility = facility,
            SerialNumber = serialNumber,
            TotalInvoiceAmount = totalInvoiceAmount,
            PurchaseDiscount = purchaseDiscount,
            OriginalRetailPrice = originalRetailPrice,
            CurrentRetailPrice = currentRetailPrice,
            StockedInDate = stockedInDate,
            LandStockNumber = landStockNumber,
            LastSyncedAtUtc = lastSyncedAtUtc
        };

        home.Raise(new OnLotHomeAddedDomainEvent { EntityId = home.Id });

        return home;
    }

    public void RevisePrice(
        decimal? totalInvoiceAmount,
        decimal? purchaseDiscount,
        decimal? originalRetailPrice,
        decimal? currentRetailPrice,
        DateTime lastSyncedAtUtc)
    {
        if (TotalInvoiceAmount == totalInvoiceAmount &&
            PurchaseDiscount == purchaseDiscount &&
            OriginalRetailPrice == originalRetailPrice &&
            CurrentRetailPrice == currentRetailPrice)
        {
            return;
        }

        TotalInvoiceAmount = totalInvoiceAmount;
        PurchaseDiscount = purchaseDiscount;
        OriginalRetailPrice = originalRetailPrice;
        CurrentRetailPrice = currentRetailPrice;
        LastSyncedAtUtc = lastSyncedAtUtc;

        Raise(new OnLotHomePriceRevisedDomainEvent { EntityId = Id });
    }

    public void ReviseDetails(
        string? stockType,
        string? condition,
        string? buildType,
        decimal? width,
        decimal? length,
        int? numberOfBedrooms,
        int? numberOfBathrooms,
        int? modelYear,
        string? model,
        string? make,
        string? facility,
        string? serialNumber,
        string? stockedInDate,
        string? landStockNumber,
        DateTime lastSyncedAtUtc)
    {
        if (StockType == stockType &&
            Condition == condition &&
            BuildType == buildType &&
            Width == width &&
            Length == length &&
            NumberOfBedrooms == numberOfBedrooms &&
            NumberOfBathrooms == numberOfBathrooms &&
            ModelYear == modelYear &&
            Model == model &&
            Make == make &&
            Facility == facility &&
            SerialNumber == serialNumber &&
            StockedInDate == stockedInDate &&
            LandStockNumber == landStockNumber)
        {
            return;
        }

        StockType = stockType;
        Condition = condition;
        BuildType = buildType;
        Width = width;
        Length = length;
        NumberOfBedrooms = numberOfBedrooms;
        NumberOfBathrooms = numberOfBathrooms;
        ModelYear = modelYear;
        Model = model;
        Make = make;
        Facility = facility;
        SerialNumber = serialNumber;
        StockedInDate = stockedInDate;
        LandStockNumber = landStockNumber;
        LastSyncedAtUtc = lastSyncedAtUtc;

        Raise(new OnLotHomeDetailsRevisedDomainEvent { EntityId = Id });
    }

    public void MarkRemoved()
    {
        Raise(new OnLotHomeRemovedDomainEvent(RefHomeCenterNumber, RefStockNumber) { EntityId = Id });
    }
}
