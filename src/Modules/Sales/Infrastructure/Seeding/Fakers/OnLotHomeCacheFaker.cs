using Bogus;
using Modules.Sales.Domain.InventoryCache;

namespace Modules.Sales.Infrastructure.Seeding.Fakers;

internal sealed class OnLotHomeCacheFaker : Faker<OnLotHomeCache>
{
    private static readonly string[] Makes = ["Clayton", "Champion", "Skyline", "Cavco", "Palm Harbor"];
    private static readonly string[] Models = ["Summit", "Freedom", "TRU", "Eclipse", "Patriot"];
    private static readonly string[] BuildTypes = ["Single", "Double", "Triple"];
    private static readonly string[] Facilities = ["Maynardville TN", "Addison AL", "Marlette MI", "Benton KY"];

    private int _refOnLotHomeId;
    private int _stockSequence;
    private int _hcIndex;

    public OnLotHomeCacheFaker(int[] homeCenterNumbers)
    {
        _refOnLotHomeId = 0;
        _stockSequence = 0;
        _hcIndex = 0;

        RuleFor(h => h.RefOnLotHomeId, _ => ++_refOnLotHomeId);
        // Round-robin home center assignment — deterministic regardless of Bogus seed.
        // Home 1 → HC 100, Home 2 → HC 200, Home 3 → HC 300, Home 4 → HC 400, Home 5 → HC 100, ...
        RuleFor(h => h.RefHomeCenterNumber, _ => homeCenterNumbers[_hcIndex++ % homeCenterNumbers.Length]);
        RuleFor(h => h.RefStockNumber, _ => $"STK{++_stockSequence:D4}");
        RuleFor(h => h.StockType, "HOME");
        RuleFor(h => h.Condition, f => f.PickRandom<HomeCondition>());
        RuleFor(h => h.BuildType, f => f.PickRandom(BuildTypes));
        RuleFor(h => h.Width, f => f.PickRandom(new decimal[] { 14m, 16m, 28m, 32m }));
        RuleFor(h => h.Length, f => f.PickRandom(56m, 60m, 64m, 72m, 76m));
        RuleFor(h => h.NumberOfBedrooms, f => f.Random.Int(2, 4));
        RuleFor(h => h.NumberOfBathrooms, f => f.Random.Int(1, 3));
        RuleFor(h => h.ModelYear, f => f.Random.Int(2024, 2026));
        RuleFor(h => h.Model, f => f.PickRandom(Models));
        RuleFor(h => h.Make, f => f.PickRandom(Makes));
        RuleFor(h => h.Facility, f => f.PickRandom(Facilities));
        RuleFor(h => h.SerialNumber, f => f.Random.Replace("CLT######AB"));
        RuleFor(h => h.OriginalRetailPrice, f => f.PickRandom(250_000m, 300_000m, 350_000m, 400_000m, 450_000m));
        // Invoice = retail minus a clean round margin ($75K / $100K / $125K).
        RuleFor(h => h.TotalInvoiceAmount, (f, h) =>
            (h.OriginalRetailPrice ?? 0m) - f.PickRandom(75_000m, 100_000m, 125_000m));
        RuleFor(h => h.CurrentRetailPrice, (f, h) => h.OriginalRetailPrice);
        RuleFor(h => h.LastSyncedAtUtc, f => f.Date.Recent(30).ToUniversalTime());
    }
}

internal static class DecimalExtensions
{
    public static decimal RoundTo(this decimal value, int decimals) => Math.Round(value, decimals);
}
