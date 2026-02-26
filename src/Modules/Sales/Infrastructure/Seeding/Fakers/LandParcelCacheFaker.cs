using Bogus;
using Modules.Sales.Domain.InventoryCache;

namespace Modules.Sales.Infrastructure.Seeding.Fakers;

internal sealed class LandParcelCacheFaker : Faker<LandParcelCache>
{
    private int _refLandParcelId;
    private int _stockSequence;
    private int _hcIndex;

    public LandParcelCacheFaker(int[] homeCenterNumbers)
    {
        _refLandParcelId = 0;
        _stockSequence = 100;
        _hcIndex = 0;

        RuleFor(l => l.RefLandParcelId, _ => ++_refLandParcelId);
        // Round-robin home center assignment — deterministic regardless of Bogus seed.
        // Parcel 1 → HC 100, Parcel 2 → HC 200, Parcel 3 → HC 300, ...
        RuleFor(l => l.RefHomeCenterNumber, _ => homeCenterNumbers[_hcIndex++ % homeCenterNumbers.Length]);
        RuleFor(l => l.RefStockNumber, _ => $"LND{++_stockSequence:D4}");
        RuleFor(l => l.StockType, "LAND");
        RuleFor(l => l.LandCost, f => f.Finance.Amount(15_000m, 60_000m));
        RuleFor(l => l.Appraisal, (f, l) => l.LandCost + f.Finance.Amount(2_000m, 15_000m));
        RuleFor(l => l.Address, f => f.Address.StreetAddress());
        RuleFor(l => l.City, f => f.Address.City());
        RuleFor(l => l.State, f => f.Address.StateAbbr());
        RuleFor(l => l.Zip, f => f.Address.ZipCode("#####"));
        RuleFor(l => l.County, f => f.Address.County());
        RuleFor(l => l.LastSyncedAtUtc, f => f.Date.Recent(30).ToUniversalTime());
    }
}
