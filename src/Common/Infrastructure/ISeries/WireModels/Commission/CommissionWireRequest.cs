namespace Rtl.Core.Infrastructure.ISeries.WireModels.Commission;

// Property names MUST match the iSeries gateway's GetCommissionRequest DTO.
internal sealed class CommissionWireRequest
{
    public int LinkId { get; set; }
    public decimal Cost { get; set; }
    public decimal LandPayoff { get; set; }
    public decimal LandImprovements { get; set; }
    public decimal AdjustedCost { get; set; }
    public int PEMPL { get; set; }
    public string HOMETYPE { get; set; } = string.Empty;
    public int MHC { get; set; }
    public CommissionSplitWire[] Splits { get; set; } = [];

    // iSeries stored procedure parameters — names must match gateway exactly
    public string PLINKYN { get; set; } = "Y";
    public string USE400C { get; set; } = "N";
    public decimal PCOM { get; set; }
    public decimal PGPAPCT { get; set; }
    public decimal PTGPPCT { get; set; }
    public string PNEWOLD { get; set; } = string.Empty;
    public decimal PCUST { get; set; }
    public string PUSER { get; set; } = string.Empty;
}

// Property names MUST match the iSeries gateway's EmployeeSplit DTO.
internal sealed class CommissionSplitWire
{
    public int EmployeeNumber { get; set; }
    public decimal Pay { get; set; }
    public decimal Gpp { get; set; }
    public decimal? TotalCommissionRate { get; set; }
}

internal sealed class CommissionWireResponse
{
    public decimal CommissionableGrossProfit { get; set; }
    public decimal TotalCommission { get; set; }
    public CommissionSplitResultWire[] EmployeeSplits { get; set; } = [];
}

internal sealed class CommissionSplitResultWire
{
    public int EmployeeNumber { get; set; }
    public decimal Pay { get; set; }
    public decimal GrossPayPercentage { get; set; }
    public decimal? TotalCommissionRate { get; set; }
}
