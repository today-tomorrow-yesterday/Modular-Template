using Modules.Sales.Domain.InventoryCache.Events;
using Rtl.Core.Domain.Auditing;
using Rtl.Core.Domain.Caching;
using Rtl.Core.Domain.Entities;

namespace Modules.Sales.Domain.InventoryCache;

// ECST cache from Inventory module — product reference table for land.
// Package lines reference this via FK (1 product : many package lines).
// Extends Entity for domain event support (appraisal change detection).
public sealed class LandParcelCache : Entity, ICacheProjection
{
    public Guid RefPublicId { get; set; }

    public int RefHomeCenterNumber { get; set; }

    public string RefStockNumber { get; set; } = string.Empty; // unique within home center

    public string? StockType { get; set; }

    [SensitiveData] public decimal? LandCost { get; set; }

    [SensitiveData] public decimal? Appraisal { get; set; }

    [SensitiveData] public string? Address { get; set; }

    [SensitiveData] public string? City { get; set; }

    [SensitiveData] public string? State { get; set; }

    [SensitiveData] public string? Zip { get; set; }

    [SensitiveData] public string? County { get; set; }

    public DateTime LastSyncedAtUtc { get; set; }

    public bool IsRemovedFromInventory { get; private set; }

    public void MarkAsRemovedFromInventory()
    {
        IsRemovedFromInventory = true;
    }

    // Applies incoming data and raises domain events for significant changes.
    // Called by the repository during upsert on existing tracked entities only.
    public void ApplyChangesFrom(LandParcelCache incoming)
    {
        var appraisalChanged =
            LandCost != incoming.LandCost ||
            Appraisal != incoming.Appraisal;

        RefHomeCenterNumber = incoming.RefHomeCenterNumber;
        RefStockNumber = incoming.RefStockNumber;
        StockType = incoming.StockType;
        LandCost = incoming.LandCost;
        Appraisal = incoming.Appraisal;
        Address = incoming.Address;
        City = incoming.City;
        State = incoming.State;
        Zip = incoming.Zip;
        County = incoming.County;
        LastSyncedAtUtc = incoming.LastSyncedAtUtc;

        if (appraisalChanged)
        {
            Raise(new LandParcelCacheAppraisalRevisedDomainEvent
            {
                LandParcelCacheId = Id,
                StockNumber = RefStockNumber
            });
        }
    }
}
