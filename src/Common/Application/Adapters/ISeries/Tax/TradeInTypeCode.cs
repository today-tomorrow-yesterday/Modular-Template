namespace Rtl.Core.Application.Adapters.ISeries.Tax;

/// <summary>
/// iSeries trade-in type codes. Used in allowance update requests.
/// The char values match the legacy iSeries lookup table.
/// </summary>
public enum TradeInTypeCode
{
    SingleWide,
    DoubleWide,
    ModularHome,
    Motorcycle,
    Boat,
    MotorVehicle,
    TravelTrailer,
    FifthWheel,
    Other
}
