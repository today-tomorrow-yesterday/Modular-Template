using Rtl.Core.Domain.Entities;

namespace Modules.Sales.Domain.Cdc;

// CDC Reference Data (Pattern A) — cdc.tax_exemption. Source: iSeries GPMSTEX.
// Authoritative — no domain table. Queries filter WHERE is_active = true.
public sealed class CdcTaxExemption : Entity
{
    public int ExemptionCode { get; set; } // iSeries: EXCODE
    public string? Description { get; set; } // iSeries: EXDESC
    public string? RulesText { get; set; } // iSeries: EXTEXT
    public bool IsActive { get; set; } // iSeries: EXSTAT

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ModifiedAtUtc { get; set; } // iSeries: EXSTCHTS
}
