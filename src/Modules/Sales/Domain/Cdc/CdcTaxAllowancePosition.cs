using Rtl.Core.Domain.Entities;

namespace Modules.Sales.Domain.Cdc;

// CDC Reference Data (Pattern A) — cdc.tax_allowance_position. Source: iSeries GPMSTAL.
// Defines allowance positions for tax calculations with GL account cross-references.
public sealed class CdcTaxAllowancePosition : Entity
{
    public int Position { get; set; } // iSeries: ALPOS
    public string Description { get; set; } = string.Empty; // iSeries: ALDSC
    public string TypeCode { get; set; } = string.Empty; // iSeries: ALTYP
    public int CategoryId { get; set; } // iSeries: ALCAT
    public int CostGlClayton { get; set; } // iSeries: ALCLC
    public int SaleGlClayton { get; set; } // iSeries: ALCLS
    public string CostGlGlobal { get; set; } = string.Empty; // iSeries: ALGBC
    public string SaleGlGlobal { get; set; } = string.Empty; // iSeries: ALGBS
    public bool IsMandatory { get; set; } // iSeries: ALMND
    public bool IsMandatorySale { get; set; } // iSeries: ALMNS

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
}
