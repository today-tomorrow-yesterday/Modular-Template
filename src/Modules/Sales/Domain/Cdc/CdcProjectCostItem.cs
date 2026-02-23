using Rtl.Core.Domain.Entities;

namespace Modules.Sales.Domain.Cdc;

// CDC Reference Data (Pattern A) — cdc.project_cost_item. Source: iSeries GPMITEM.
// Multi-tenant: queries filter WHERE master_dealer = 29.
// Individual cost line items within a category (e.g., "Skirting", "AC Unit").
public sealed class CdcProjectCostItem : Entity
{
    public int MasterDealer { get; set; } // iSeries: MDLR
    public int ProjectCostCategoryId { get; set; } // FK -> CdcProjectCostCategory.Id
    public int CategoryId { get; set; } // iSeries: CATID
    public int ItemNumber { get; set; } // iSeries: ITEMID
    public string Description { get; set; } = string.Empty; // iSeries: DSCRPT
    public string Status { get; set; } = string.Empty; // iSeries: CISTAT
    public bool IsFeeItem { get; set; } // iSeries: ITFEE
    public bool IsCssRestricted { get; set; } // iSeries: ITCSS
    public bool IsFhaRestricted { get; set; } // iSeries: ITFHA
    public bool IsDisplayForCash { get; set; } // iSeries: ITCASH
    public bool IsRestrictOptionPrice { get; set; } // iSeries: ITSLSP
    public bool IsRestrictCssCost { get; set; } // iSeries: ITCOST
    public bool IsHopeRefundsIncluded { get; set; } // iSeries: ITHOPE
    public decimal? ProfitPercentage { get; set; } // iSeries: ITPROF

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }

    // Navigation
    public CdcProjectCostCategory Category { get; set; } = null!;
}
