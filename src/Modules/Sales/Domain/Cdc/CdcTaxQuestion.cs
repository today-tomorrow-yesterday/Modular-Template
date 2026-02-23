using Rtl.Core.Domain.Entities;

namespace Modules.Sales.Domain.Cdc;

// CDC Reference Data (Pattern A) — cdc.tax_question. Source: iSeries GPMSTQR.
// Multi-tenant: master_dealer IN (0, 15, 29). Queries filter WHERE master_dealer = 29.
// State-specific questions controlling which tax questions appear on the package tax section.
public sealed class CdcTaxQuestion : Entity
{
    public int MasterDealer { get; set; } // iSeries: QMDLR
    public string StateCode { get; set; } = string.Empty; // iSeries: QSTATE
    public int QuestionNumber { get; set; } // iSeries: QQUES#
    public DateOnly EffectiveDate { get; set; } // iSeries: QEFFDT
    public DateOnly? EndDate { get; set; } // iSeries: QENDDT
    public bool AskForNew { get; set; } // iSeries: QNEW
    public bool AskForUsed { get; set; } // iSeries: QUSED
    public bool AskForRepo { get; set; } // iSeries: QREPO
    public bool AskForLand { get; set; } // iSeries: QLAND

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }

    // Navigation
    public ICollection<CdcTaxQuestionText> QuestionTexts { get; set; } = [];
}
