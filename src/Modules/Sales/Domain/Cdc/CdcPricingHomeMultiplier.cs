using Rtl.Core.Domain.Entities;

namespace Modules.Sales.Domain.Cdc;

// CDC Reference Data (Pattern A) — cdc.pricing_home_multiplier. Source: iSeries GPMSTHM.
// State-level multipliers applied to home pricing calculations (retail price, freight, upgrades, W&A, dues).
// Queries filter WHERE is_active = true.
public sealed class CdcPricingHomeMultiplier : Entity
{
    public int Homident { get; set; } // iSeries: HOMIDENT
    public string StateCode { get; set; } = string.Empty; // iSeries: HOMSTATE
    public DateOnly EffectiveDate { get; set; } // iSeries: HOMEFFDT
    public decimal HomeMultiplierValue { get; set; } // iSeries: HOMHMULT
    public decimal FreightMultiplier { get; set; } // iSeries: HOMFMULT
    public decimal UpgradesMultiplier { get; set; } // iSeries: HOMUMULT
    public decimal WheelsAxlesMultiplier { get; set; } // iSeries: HOMWMULT
    public decimal DuesMultiplier { get; set; } // iSeries: HOMDMULT
    public bool IsActive { get; set; } // iSeries: HOMROWST

    public DateTime CreatedAtUtc { get; set; } // iSeries: HOMCRTTS
    public DateTime? ModifiedAtUtc { get; set; } // iSeries: HOMUPDTS
}
