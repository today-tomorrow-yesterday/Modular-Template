using Rtl.Core.Domain.Entities;

namespace Modules.Sales.Domain.Cdc;

// CDC Reference Data (Pattern A) — cdc.tax_question_text. Source: iSeries GPMSTQT.
// Display text for tax questions. FK to CdcTaxQuestion.
public sealed class CdcTaxQuestionText : Entity
{
    public int TaxQuestionId { get; set; } // FK -> CdcTaxQuestion.Id
    public int QuestionNumber { get; set; } // iSeries: QQUES#
    public string Text { get; set; } = string.Empty; // iSeries: QTEXT
    public bool IsActive { get; set; } // iSeries: QSTATUS
    public DateOnly? InactivateDate { get; set; } // iSeries: QIDATE
    public string? InactivatedBy { get; set; } // iSeries: QIUSER

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }

    // Navigation
    public CdcTaxQuestion TaxQuestion { get; set; } = null!;
}
