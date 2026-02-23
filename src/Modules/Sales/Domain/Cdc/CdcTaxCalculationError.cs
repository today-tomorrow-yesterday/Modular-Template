using Modules.Sales.Domain.FundingCache;
using Rtl.Core.Domain.Entities;

namespace Modules.Sales.Domain.Cdc;

// CDC Reference Data (Pattern A) — cdc.tax_calculation_error. Source: iSeries GPMSTFP.
// Tax calculation errors linked to funding requests. FK to cache.funding_requests_cache.
public sealed class CdcTaxCalculationError : Entity
{
    public int FundingId { get; set; } // FK -> FundingRequestCache.Id
    public int? LinkId { get; set; } // iSeries: FPLINK
    public int SequenceNumber { get; set; } // iSeries: FPSEQ
    public string Message { get; set; } = string.Empty; // iSeries: FPMSG
    public int MasterDealer { get; set; } // iSeries: FPMDLR
    public int HomeCenterNumber { get; set; } // iSeries: FPLOT
    public string FieldName { get; set; } = string.Empty; // iSeries: FPFLD
    public string MessageId { get; set; } = string.Empty; // iSeries: MSGID
    public string ProgramName { get; set; } = string.Empty; // iSeries: FPPGM

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }

    // Navigation
    public FundingRequestCache FundingRequest { get; set; } = null!;
}
