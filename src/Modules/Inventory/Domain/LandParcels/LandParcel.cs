using Modules.Inventory.Domain.LandParcels.Events;
using Rtl.Core.Domain.Auditing;
using Rtl.Core.Domain.Entities;

namespace Modules.Inventory.Domain.LandParcels;

public sealed class LandParcel : Entity, IAggregateRoot
{
    private LandParcel() { }

    public Guid PublicId { get; private set; }

    public int RefHomeCenterNumber { get; private set; }

    public string RefStockNumber { get; private set; } = string.Empty;

    public string? StockType { get; private set; }

    public string? LandAge { get; private set; }

    [SensitiveData] public decimal? LandCost { get; private set; }

    [SensitiveData] public decimal? AddToTotal { get; private set; }

    [SensitiveData] public decimal? Appraisal { get; private set; }

    public string? MapParcel { get; private set; }

    [SensitiveData] public string? Address { get; private set; }

    [SensitiveData] public string? Address2 { get; private set; }

    [SensitiveData] public string? City { get; private set; }

    [SensitiveData] public string? State { get; private set; }

    [SensitiveData] public string? Zip { get; private set; }

    [SensitiveData] public string? County { get; private set; }

    [SensitiveData] public string? LoanNumber { get; private set; }

    public string? HomeStockNumber { get; private set; }

    public DateTime LastSyncedAtUtc { get; private set; }

    public static LandParcel Create(
        int id,
        int refHomeCenterNumber,
        string refStockNumber,
        string? stockType,
        string? landAge,
        decimal? landCost,
        decimal? addToTotal,
        decimal? appraisal,
        string? mapParcel,
        string? address,
        string? address2,
        string? city,
        string? state,
        string? zip,
        string? county,
        string? loanNumber,
        string? homeStockNumber,
        DateTime lastSyncedAtUtc)
    {
        var parcel = new LandParcel
        {
            Id = id,
            PublicId = Guid.CreateVersion7(),
            RefHomeCenterNumber = refHomeCenterNumber,
            RefStockNumber = refStockNumber,
            StockType = stockType,
            LandAge = landAge,
            LandCost = landCost,
            AddToTotal = addToTotal,
            Appraisal = appraisal,
            MapParcel = mapParcel,
            Address = address,
            Address2 = address2,
            City = city,
            State = state,
            Zip = zip,
            County = county,
            LoanNumber = loanNumber,
            HomeStockNumber = homeStockNumber,
            LastSyncedAtUtc = lastSyncedAtUtc
        };

        parcel.Raise(new LandParcelAddedDomainEvent { EntityId = parcel.Id });

        return parcel;
    }

    public void ReviseAppraisal(
        decimal? landCost,
        decimal? appraisal,
        DateTime lastSyncedAtUtc)
    {
        if (LandCost == landCost &&
            Appraisal == appraisal)
        {
            return;
        }

        LandCost = landCost;
        Appraisal = appraisal;
        LastSyncedAtUtc = lastSyncedAtUtc;

        Raise(new LandParcelAppraisalRevisedDomainEvent { EntityId = Id });
    }

    public void ReviseDetails(
        string? stockType,
        string? landAge,
        decimal? addToTotal,
        string? mapParcel,
        string? address,
        string? address2,
        string? city,
        string? state,
        string? zip,
        string? county,
        string? loanNumber,
        string? homeStockNumber,
        DateTime lastSyncedAtUtc)
    {
        if (StockType == stockType &&
            LandAge == landAge &&
            AddToTotal == addToTotal &&
            MapParcel == mapParcel &&
            Address == address &&
            Address2 == address2 &&
            City == city &&
            State == state &&
            Zip == zip &&
            County == county &&
            LoanNumber == loanNumber &&
            HomeStockNumber == homeStockNumber)
        {
            return;
        }

        StockType = stockType;
        LandAge = landAge;
        AddToTotal = addToTotal;
        MapParcel = mapParcel;
        Address = address;
        Address2 = address2;
        City = city;
        State = state;
        Zip = zip;
        County = county;
        LoanNumber = loanNumber;
        HomeStockNumber = homeStockNumber;
        LastSyncedAtUtc = lastSyncedAtUtc;

        Raise(new LandParcelDetailsRevisedDomainEvent { EntityId = Id });
    }

    public void MarkRemoved()
    {
        Raise(new LandParcelRemovedDomainEvent(PublicId) { EntityId = Id });
    }
}
