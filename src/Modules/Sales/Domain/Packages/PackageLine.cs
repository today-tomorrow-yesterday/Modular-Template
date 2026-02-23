using Rtl.Core.Domain.Auditing;
using Rtl.Core.Domain.Entities;

namespace Modules.Sales.Domain.Packages;

public enum Responsibility
{
    Buyer,
    Seller
}

// Abstract base — packages.lines. All 9 section types in a single table (TPH).
// Common pricing columns are shared. Type-specific data lives in the Details JSONB column
// via the generic PackageLine<TDetails> intermediate, deserialized to typed DTOs per line type.
public abstract class PackageLine : AuditableEntity
{
    protected PackageLine() { }

    public int PackageId { get; protected set; }
    public string LineType { get; protected set; } = string.Empty; // TPH discriminator

    [SensitiveData] public decimal SalePrice { get; protected set; }
    [SensitiveData] public decimal EstimatedCost { get; protected set; }
    [SensitiveData] public decimal RetailSalePrice { get; protected set; } // SalePrice <= RetailSalePrice
    public Responsibility? Responsibility { get; protected set; }
    public virtual bool ShouldExcludeFromPricing { get; protected set; }
    public int SortOrder { get; protected set; }

    public Package Package { get; private set; } = null!;

    public void UpdatePricing(decimal salePrice, decimal estimatedCost, decimal retailSalePrice)
    {
        SalePrice = Math.Round(salePrice, 2);
        EstimatedCost = Math.Round(estimatedCost, 2);
        RetailSalePrice = Math.Round(retailSalePrice, 2);
    }
}

// Generic intermediate — typed Details property for each line type.
// EF Core maps TDetails via OwnsOne/ToJson("details") per concrete derived type.
public abstract class PackageLine<TDetails> : PackageLine where TDetails : class
{
    protected PackageLine() { }

    public TDetails? Details { get; protected set; }

    public void UpdateDetails(TDetails? details) => Details = details;
}
