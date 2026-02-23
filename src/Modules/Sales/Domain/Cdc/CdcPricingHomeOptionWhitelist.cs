using Rtl.Core.Domain.Entities;

namespace Modules.Sales.Domain.Cdc;

// CDC Reference Data (Pattern A) — cdc.pricing_home_option_whitelist. Source: iSeries GPMSTHO.
// Whitelist of home options eligible for multiplier-based pricing.
// PlantNumber+OptionNumber combination determines which factory options get price multipliers applied.
public sealed class CdcPricingHomeOptionWhitelist : Entity
{
    public int Howident { get; set; } // iSeries: HOWIDENT
    public int PlantNumber { get; set; } // iSeries: HOWPLANT
    public int OptionNumber { get; set; } // iSeries: HOWOPTNO
    public string MultiplierCode { get; set; } = string.Empty; // iSeries: HOWOPMCD
    public string Status { get; set; } = string.Empty; // iSeries: HOWSTAT
    public DateOnly EffectiveDate { get; set; } // iSeries: HOWEFFDT

    public DateTime CreatedAtUtc { get; set; } // iSeries: HOWCRTTS
    public DateTime? ModifiedAtUtc { get; set; } // iSeries: HOWUPDTS
}
