using Modules.Sales.Domain.RetailLocations;

namespace Modules.Sales.Infrastructure.Seeding.Fakers;

internal static class RetailLocationFaker
{
    private static readonly (int Number, string Name, string State, string Zip, bool IsActive)[] HomeCenters =
    [
        (100, "Maryville Home Center", "TN", "37801", true),
        (200, "Knoxville Home Center", "TN", "37920", true),
        (300, "Birmingham Home Center", "AL", "35212", true),
        (400, "Hilton Head Home Center", "SC", "29915", true),
        (500, "Tallahassee Home Center", "FL", "32304", false)
    ];

    public static List<RetailLocation> Generate()
    {
        return HomeCenters
            .Select(hc => RetailLocation.CreateHomeCenter(
                hc.Number, hc.Name, hc.State, hc.Zip, hc.IsActive))
            .ToList();
    }

    public static int[] GetHomeCenterNumbers() =>
        HomeCenters.Where(hc => hc.IsActive).Select(hc => hc.Number).ToArray();
}
