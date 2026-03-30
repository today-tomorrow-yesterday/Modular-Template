using Modules.Sales.Domain.Packages.Credits;
using Modules.Sales.Domain.Packages.Events;
using Modules.Sales.Domain.Packages.Home;
using Modules.Sales.Domain.Packages.Insurance;
using Modules.Sales.Domain.Packages.Land;
using Modules.Sales.Domain.Packages.ProjectCosts;
using Modules.Sales.Domain.Sales;
using Rtl.Core.Domain.Auditing;
using Rtl.Core.Domain.Entities;

namespace Modules.Sales.Domain.Packages;

public enum PackageStatus
{
    Draft
}

// Aggregate root — packages.packages. A home package containing all pricing lines for a sale.
// Packages are ranked; the primary package (Ranking == 1) is the active one.
// PublicId (UUID v7) used in API routes.
public sealed class Package : AuditableEntity, IAggregateRoot
{
    private readonly List<PackageLine> _lines = [];

    private Package() { }

    public Guid PublicId { get; private set; }
    public int? Version { get; private set; } // Optimistic concurrency
    public int SaleId { get; private set; }
    public int Ranking { get; private set; } // 1 = primary, 2+ = alternates
    public PackageStatus Status { get; private set; }
    public string Name { get; private set; } = string.Empty; // Unique within a sale (case-insensitive)
    [SensitiveData] public decimal GrossProfit { get; private set; }
    [SensitiveData] public decimal CommissionableGrossProfit { get; private set; }
    public bool MustRecalculateTaxes { get; private set; }

    public bool IsPrimaryPackage => Ranking == 1;

    public Sale Sale { get; private set; } = null!;
    public IReadOnlyCollection<PackageLine> Lines => _lines.AsReadOnly();

    public static Package Create(int saleId, string name, bool isPrimary)
    {
        var package = new Package
        {
            PublicId = Guid.CreateVersion7(),
            SaleId = saleId,
            Name = name,
            Ranking = isPrimary ? 1 : 2,
            Status = PackageStatus.Draft,
            GrossProfit = 0m,
            CommissionableGrossProfit = 0m,
            MustRecalculateTaxes = true
        };

        package.Raise(new PackageReadyForFundingDomainEvent
        {
            SaleId = saleId,
            PackagePublicId = package.PublicId,
            RequestAmount = 0m
        });

        return package;
    }

    // --- Package metadata ---

    public void SetName(string name) => Name = name;

    public void SetPrimary() => Ranking = 1;

    public void SetNonPrimary(int ranking) => Ranking = ranking;

    public void SetCommissionableGrossProfit(decimal value) => CommissionableGrossProfit = value;

    // Tax recalculation is driven by handler-level change detection (snapshot → mutate → compare).
    // The aggregate owns the flag; handlers decide when to set it based on domain-specific rules.
    public void FlagForTaxRecalculation() => MustRecalculateTaxes = true;

    public void ClearTaxRecalculationFlag() => MustRecalculateTaxes = false;

    // --- Line mutations ---
    // All mutations auto-recalculate gross profit to keep the aggregate consistent.

    public void AddLine(PackageLine line)
    {
        _lines.Add(line);
        RecalculateGrossProfit();

        switch (line)
        {
            case HomeLine:
                Raise(new HomeLineUpdatedDomainEvent { SaleId = SaleId, PackageId = Id });
                break;
            case LandLine:
                Raise(new LandLineUpdatedDomainEvent { SaleId = SaleId, PackageId = Id });
                break;
        }
    }

    public void RemoveLine(PackageLine line)
    {
        _lines.Remove(line);
        RecalculateGrossProfit();
    }

    public T? RemoveLine<T>(Func<T, bool>? predicate = null) where T : PackageLine
    {
        var line = predicate is null
            ? _lines.OfType<T>().SingleOrDefault()
            : _lines.OfType<T>().SingleOrDefault(predicate);
        if (line is null) return null;

        _lines.Remove(line);
        RecalculateGrossProfit();
        return line;
    }

    // Removes all lines of the given type. Returns the count removed.
    public int RemoveAllLines<T>() where T : PackageLine
    {
        var count = _lines.RemoveAll(l => l is T);
        if (count > 0) RecalculateGrossProfit();
        return count;
    }

    // --- Domain-specific line removal ---

    public InsuranceLine? RemoveHomeFirstInsuranceLine() =>
        RemoveLine<InsuranceLine>(l => l.Details?.InsuranceType == InsuranceType.HomeFirst);

    public InsuranceLine? RemoveOutsideInsuranceLine() =>
        RemoveLine<InsuranceLine>(l => l.Details?.InsuranceType == InsuranceType.Outside);

    public CreditLine? RemoveDownPaymentLine() =>
        RemoveLine<CreditLine>(l => l.IsDownPayment);

    public CreditLine? RemoveConcessionLine() =>
        RemoveLine<CreditLine>(l => l.IsConcession);

    public ProjectCostLine? RemoveProjectCost(int categoryId, int itemId) =>
        RemoveLine<ProjectCostLine>(l => l.Details?.CategoryId == categoryId && l.Details?.ItemId == itemId);

    public int RemoveProjectCostsByCategory(int categoryId)
    {
        var count = _lines.RemoveAll(l => l is ProjectCostLine pc && pc.Details?.CategoryId == categoryId);
        if (count > 0) RecalculateGrossProfit();
        return count;
    }

    public int RemoveAllProjectCosts(int categoryId, int itemId)
    {
        var count = _lines.RemoveAll(l =>
            l is ProjectCostLine pc
            && pc.Details?.CategoryId == categoryId
            && pc.Details?.ItemId == itemId);
        if (count > 0) RecalculateGrossProfit();
        return count;
    }

    // --- Product unavailability ---

    // Marks a package line's inventory product as removed and raises a domain event.
    // Called when Inventory removes a home or land parcel from its catalog.
    public void MarkLineProductUnavailable(PackageLine line, string productType, string? stockNumber)
    {
        line.IsProductRemovedFromInventory = true;

        Raise(new ProductRemovedFromInventoryDomainEvent
        {
            PackageId = Id,
            PackagePublicId = PublicId,
            SaleId = SaleId,
            PackageLineId = line.Id,
            ProductType = productType,
            StockNumber = stockNumber
        });
    }

    // --- Gross profit ---
    // Auto-called by all line mutations. Kept public for backward compat — idempotent, harmless to call again.

    public void RecalculateGrossProfit()
    {
        GrossProfit = _lines
            .Where(l => !l.ShouldExcludeFromPricing)
            .Sum(l => l.SalePrice - l.EstimatedCost);
    }


}
