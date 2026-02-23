namespace Rtl.Core.Application.Adapters.ISeries.Pricing;

public sealed class WheelAndAxlePriceByCountRequest
{
    public int NumberOfWheels { get; init; }
    public int NumberOfAxles { get; init; }
}
