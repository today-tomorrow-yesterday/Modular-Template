using Modules.Sales.Domain.RetailLocations;

namespace Modules.Sales.Infrastructure.Seeding.Fakers;

internal static class RetailLocationFaker
{
    private static readonly (int Number, string Name, string State, string Zip, bool IsActive)[] HomeCenters =
    [
        (100, "Springfield Home Center", "OH", "45502", true),
        (200, "Indianapolis Home Center", "IN", "46201", true),
        (300, "Dallas Home Center", "TX", "75201", true),
        (400, "Charlotte Home Center", "NC", "28201", true),
        (500, "Orlando Home Center", "FL", "32801", false)
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
