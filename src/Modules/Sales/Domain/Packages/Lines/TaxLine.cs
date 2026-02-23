using Modules.Sales.Domain.Packages.Details;

namespace Modules.Sales.Domain.Packages.Lines;

// Tax line — calculated taxes for the package. 1:1 per package.
// ShouldExcludeFromPricing is configurable (some tax scenarios excluded).
public sealed class TaxLine : PackageLine<TaxDetails>
{
    private TaxLine() { }

    public static TaxLine Create(
        int packageId,
        decimal salePrice,
        decimal estimatedCost,
        decimal retailSalePrice,
        bool shouldExcludeFromPricing,
        TaxDetails? details)
    {
        return new TaxLine
        {
            PackageId = packageId,
            LineType = PackageLineTypeConstants.Tax,
            SalePrice = Math.Round(salePrice, 2),
            EstimatedCost = Math.Round(estimatedCost, 2),
            RetailSalePrice = Math.Round(retailSalePrice, 2),
            ShouldExcludeFromPricing = shouldExcludeFromPricing,
            Details = details
        };
    }

    // Clears state-specific tax question answers — preserves config + calculations
    public void ClearQuestionAnswers()
    {
        if (Details is not null)
        {
            Details = Details.WithClearedQuestionAnswers();
        }
    }

    // Clears calculated tax data and resets SalePrice — preserves config + question answers
    public void ClearCalculations()
    {
        if (Details is not null)
        {
            Details = Details.WithClearedCalculations();
        }

        SalePrice = 0;
    }

    // Clears errors only — preserves config, question answers, and tax calculations
    public void ClearErrors()
    {
        if (Details is not null)
        {
            Details = Details.WithClearedErrors();
        }
    }

    // Clears PreviouslyTitled — called when home type changes (user must re-answer)
    public void ClearPreviouslyTitled()
    {
        if (Details is not null)
        {
            Details = Details.WithClearedPreviouslyTitled();
        }
    }
}
