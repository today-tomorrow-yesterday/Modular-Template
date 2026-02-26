using Rtl.Core.Domain.Entities;
using System.Text.Json;

namespace Modules.Sales.Domain.Packages.Tax;

public sealed class TaxDetails : IVersionedDetails
{
    public int SchemaVersion => 1;
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }

    public string? PreviouslyTitled { get; private set; }
    public int? TaxExemptionId { get; private set; }
    public List<TaxQuestionAnswer> StateTaxQuestionAnswers { get; private set; } = [];
    public List<TaxItem> Taxes { get; private set; } = [];
    public List<string>? Errors { get; private set; }

    // Tax exemption snapshot (from cdc.tax_exemption at selection time)
    public string? TaxExemptionDescription { get; private set; }

    // Location context at calculation time
    public string? StateCode { get; private set; }
    public string? DeliveryCity { get; private set; }
    public string? DeliveryCounty { get; private set; }
    public string? DeliveryPostalCode { get; private set; }
    public bool? DeliveryIsWithinCityLimits { get; private set; }

    // Selective clearing — preserves config fields (PreviouslyTitled, TaxExemptionId)
    public TaxDetails WithClearedQuestionAnswers()
    {
        return new TaxDetails
        {
            PreviouslyTitled = PreviouslyTitled,
            TaxExemptionId = TaxExemptionId,
            TaxExemptionDescription = TaxExemptionDescription,
            StateCode = StateCode,
            DeliveryCity = DeliveryCity,
            DeliveryCounty = DeliveryCounty,
            DeliveryPostalCode = DeliveryPostalCode,
            DeliveryIsWithinCityLimits = DeliveryIsWithinCityLimits,
            StateTaxQuestionAnswers = [],
            Taxes = [.. Taxes],
            Errors = Errors
        };
    }

    // Clears calculated results — preserves all config + question answers
    public TaxDetails WithClearedCalculations()
    {
        return new TaxDetails
        {
            PreviouslyTitled = PreviouslyTitled,
            TaxExemptionId = TaxExemptionId,
            TaxExemptionDescription = TaxExemptionDescription,
            StateCode = StateCode,
            DeliveryCity = DeliveryCity,
            DeliveryCounty = DeliveryCounty,
            DeliveryPostalCode = DeliveryPostalCode,
            DeliveryIsWithinCityLimits = DeliveryIsWithinCityLimits,
            StateTaxQuestionAnswers = [.. StateTaxQuestionAnswers],
            Taxes = [],
            Errors = null
        };
    }

    // Clears errors only — preserves all config, question answers, and tax calculations
    public TaxDetails WithClearedErrors()
    {
        return new TaxDetails
        {
            PreviouslyTitled = PreviouslyTitled,
            TaxExemptionId = TaxExemptionId,
            TaxExemptionDescription = TaxExemptionDescription,
            StateCode = StateCode,
            DeliveryCity = DeliveryCity,
            DeliveryCounty = DeliveryCounty,
            DeliveryPostalCode = DeliveryPostalCode,
            DeliveryIsWithinCityLimits = DeliveryIsWithinCityLimits,
            StateTaxQuestionAnswers = [.. StateTaxQuestionAnswers],
            Taxes = [.. Taxes],
            Errors = null
        };
    }

    // Clears PreviouslyTitled — called when home type changes (user must re-answer)
    public TaxDetails WithClearedPreviouslyTitled()
    {
        return new TaxDetails
        {
            PreviouslyTitled = null,
            TaxExemptionId = TaxExemptionId,
            TaxExemptionDescription = TaxExemptionDescription,
            StateCode = StateCode,
            DeliveryCity = DeliveryCity,
            DeliveryCounty = DeliveryCounty,
            DeliveryPostalCode = DeliveryPostalCode,
            DeliveryIsWithinCityLimits = DeliveryIsWithinCityLimits,
            StateTaxQuestionAnswers = [.. StateTaxQuestionAnswers],
            Taxes = [.. Taxes],
            Errors = Errors
        };
    }

    private TaxDetails() { }

    public static TaxDetails Create(
        string? previouslyTitled,
        int? taxExemptionId,
        List<TaxQuestionAnswer> questionAnswers,
        List<TaxItem> taxes,
        List<string>? errors,
        string? taxExemptionDescription = null,
        string? stateCode = null,
        string? deliveryCity = null,
        string? deliveryCounty = null,
        string? deliveryPostalCode = null,
        bool? deliveryIsWithinCityLimits = null)
    {
        return new TaxDetails
        {
            PreviouslyTitled = previouslyTitled,
            TaxExemptionId = taxExemptionId,
            StateTaxQuestionAnswers = [.. questionAnswers],
            Taxes = [.. taxes],
            Errors = errors?.ToList(),
            TaxExemptionDescription = taxExemptionDescription,
            StateCode = stateCode,
            DeliveryCity = deliveryCity,
            DeliveryCounty = deliveryCounty,
            DeliveryPostalCode = deliveryPostalCode,
            DeliveryIsWithinCityLimits = deliveryIsWithinCityLimits
        };
    }
}

public sealed class TaxItem
{
    public string Name { get; private set; } = string.Empty;
    public bool IsOverridden { get; private set; }
    public decimal? CalculatedAmount { get; private set; } // iSeries-calculated
    public decimal? ChargedAmount { get; private set; } // May differ from calculated if overridden

    public static TaxItem Create(string name, decimal calculatedAmount, decimal? chargedAmount = null)
    {
        return new TaxItem
        {
            Name = name,
            IsOverridden = chargedAmount.HasValue && chargedAmount != calculatedAmount,
            CalculatedAmount = calculatedAmount,
            ChargedAmount = chargedAmount ?? calculatedAmount
        };
    }
}

public sealed class TaxQuestionAnswer
{
    public int QuestionNumber { get; private set; } // Natural key to cdc.tax_questions
    public string? Answer { get; private set; } // Serialized as string — original values may be string, int, or bool
    public string? QuestionText { get; private set; } // Frozen at answer time from cdc.tax_question_text

    public static TaxQuestionAnswer Create(int questionNumber, string? answer, string? questionText = null)
    {
        return new TaxQuestionAnswer
        {
            QuestionNumber = questionNumber,
            Answer = answer,
            QuestionText = questionText
        };
    }
}
