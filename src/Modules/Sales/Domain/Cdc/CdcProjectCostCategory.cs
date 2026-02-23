using Rtl.Core.Domain.Entities;

namespace Modules.Sales.Domain.Cdc;

// CDC Reference Data (Pattern A) — cdc.project_cost_category. Source: iSeries GPMCATG.
// Multi-tenant: queries filter WHERE master_dealer = 29.
// Categories group project cost items (e.g., "Setup", "Delivery", "Options").
public sealed class CdcProjectCostCategory : Entity
{
    public int MasterDealer { get; set; } // iSeries: MDLR
    public int CategoryNumber { get; set; } // iSeries: CATID
    public string Description { get; set; } = string.Empty; // iSeries: DSCRPT
    public bool IsCreditConsideration { get; set; } // iSeries: DSCRDT
    public bool IsLandDot { get; set; } // iSeries: DSLAND
    public bool RestrictFha { get; set; } // iSeries: DSFHA
    public bool RestrictCss { get; set; } // iSeries: DSCSS
    public bool DisplayForCash { get; set; } // iSeries: DSCASH

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }

    // Navigation
    public ICollection<CdcProjectCostItem> Items { get; set; } = [];
}
