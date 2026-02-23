using Rtl.Core.Domain.Entities;
using System.Text.Json;

namespace Modules.Sales.Domain.Packages.Details;

public sealed class WarrantyDetails : IVersionedDetails
{
    private WarrantyDetails() { }

    public int SchemaVersion => 1;
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }

    public bool WarrantySelected { get; private set; } // Whether warranty is selected — tax-affecting field
    public decimal? WarrantyAmount { get; private set; } // Warranty premium amount — tax-affecting field
    public decimal? SalesTaxPremium { get; private set; } // Sales tax on the warranty premium
    public DateTimeOffset? QuotedAt { get; private set; } // When the quote was generated

    public static WarrantyDetails Create(
        decimal warrantyAmount,
        decimal salesTaxPremium,
        bool warrantySelected = true)
    {
        return new WarrantyDetails
        {
            WarrantySelected = warrantySelected,
            WarrantyAmount = warrantyAmount,
            SalesTaxPremium = salesTaxPremium,
            QuotedAt = DateTimeOffset.UtcNow
        };
    }
}
