using Rtl.Core.Domain.Entities;

namespace Modules.Sales.Domain.Cdc;

// CDC Reference Data (Pattern A) — cdc.project_cost_state_matrix. Source: iSeries GPMMTRX.
// Multi-tenant: queries filter WHERE master_dealer = 29.
// State-specific tax basis and insurability rules for each category+item+homeType+state combination.
public sealed class CdcProjectCostStateMatrix : Entity
{
    public int MasterDealer { get; set; } // iSeries: MDLR
    public int ProjectCostCategoryId { get; set; } // FK -> CdcProjectCostCategory.Id
    public int ProjectCostItemId { get; set; } // FK -> CdcProjectCostItem.Id
    public int CategoryId { get; set; } // iSeries: CATID
    public int CategoryItemId { get; set; } // iSeries: ITEMID
    public string HomeType { get; set; } = string.Empty; // iSeries: TYPE
    public string StateCode { get; set; } = string.Empty; // iSeries: STATE
    public decimal TaxBasisManufactured { get; set; } // iSeries: MFG
    public decimal TaxBasisModularOn { get; set; } // iSeries: MODON
    public decimal TaxBasisModularOff { get; set; } // iSeries: MODOFF
    public bool IsInsurable { get; set; } // iSeries: INS
    public bool? IsAdjStructInsurable { get; set; } // iSeries: ADJINS
    public bool? IsTotalImprovementIncluded { get; set; } // iSeries: TTLIMP
    public bool? IsFeeItemAllowed { get; set; } // iSeries: ALWFEE

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }

    // Navigation
    public CdcProjectCostCategory Category { get; set; } = null!;
    public CdcProjectCostItem Item { get; set; } = null!;
}
