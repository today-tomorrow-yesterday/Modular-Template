using Modules.Sales.Domain.InventoryCache.Events;
using Rtl.Core.Domain.Auditing;
using Rtl.Core.Domain.Caching;
using Rtl.Core.Domain.Entities;

namespace Modules.Sales.Domain.InventoryCache;

public enum HomeCondition
{
    New,
    Used,
    Repo
}

// ECST cache from Inventory module — product reference table for homes.
// Package lines reference this via FK (1 product : many package lines).
// Extends Entity for domain event support (price change detection).
public sealed class OnLotHomeCache : Entity, ICacheProjection
{
    public int RefOnLotHomeId { get; set; }

    public int RefHomeCenterNumber { get; set; }

    public string RefStockNumber { get; set; } = string.Empty; // unique within home center

    public string? StockType { get; set; }

    public HomeCondition? Condition { get; set; }

    public string? BuildType { get; set; }

    public decimal? Width { get; set; }

    public decimal? Length { get; set; }

    public int? NumberOfBedrooms { get; set; }

    public int? NumberOfBathrooms { get; set; }

    public int? ModelYear { get; set; }

    public string? Model { get; set; }

    public string? Make { get; set; }

    public string? Facility { get; set; }

    [SensitiveData] public string? SerialNumber { get; set; }

    [SensitiveData] public decimal? TotalInvoiceAmount { get; set; }

    [SensitiveData] public decimal? OriginalRetailPrice { get; set; }

    [SensitiveData] public decimal? CurrentRetailPrice { get; set; }

    public DateTime LastSyncedAtUtc { get; set; }

    // Applies incoming data and raises domain events for significant changes.
    // Called by the repository during upsert on existing tracked entities only.
    public void ApplyChangesFrom(OnLotHomeCache incoming)
    {
        var priceChanged =
            CurrentRetailPrice != incoming.CurrentRetailPrice ||
            OriginalRetailPrice != incoming.OriginalRetailPrice ||
            TotalInvoiceAmount != incoming.TotalInvoiceAmount;

        RefHomeCenterNumber = incoming.RefHomeCenterNumber;
        RefStockNumber = incoming.RefStockNumber;
        StockType = incoming.StockType;
        Condition = incoming.Condition;
        BuildType = incoming.BuildType;
        Width = incoming.Width;
        Length = incoming.Length;
        NumberOfBedrooms = incoming.NumberOfBedrooms;
        NumberOfBathrooms = incoming.NumberOfBathrooms;
        ModelYear = incoming.ModelYear;
        Model = incoming.Model;
        Make = incoming.Make;
        Facility = incoming.Facility;
        SerialNumber = incoming.SerialNumber;
        TotalInvoiceAmount = incoming.TotalInvoiceAmount;
        OriginalRetailPrice = incoming.OriginalRetailPrice;
        CurrentRetailPrice = incoming.CurrentRetailPrice;
        LastSyncedAtUtc = incoming.LastSyncedAtUtc;

        if (priceChanged)
        {
            Raise(new OnLotHomeCachePriceRevisedDomainEvent
            {
                OnLotHomeCacheId = Id,
                StockNumber = RefStockNumber
            });
        }
    }
}
