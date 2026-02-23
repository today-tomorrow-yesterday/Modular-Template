using Rtl.Core.Domain.Results;

namespace Modules.Inventory.Application.Transportation.GetTransportation;

internal static class TransportationErrors
{
    internal static Error NoDimensionData(decimal length, decimal width) =>
        Error.NotFound(
            "Transportation.NoDimensionData",
            $"No transportation data found for dimensions {length}x{width}.");
}
