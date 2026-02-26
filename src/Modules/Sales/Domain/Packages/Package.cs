using Modules.Sales.Domain.Packages.Details;
using Modules.Sales.Domain.Packages.Events;
using Modules.Sales.Domain.Packages.Lines;
using Modules.Sales.Domain.Sales;
using Rtl.Core.Domain.Auditing;
using Rtl.Core.Domain.Entities;

namespace Modules.Sales.Domain.Packages;

public enum PackageStatus
{
    Draft
}

// Entity — packages.packages. A home package containing all pricing lines for a sale.
// Packages are ranked; the primary package (Ranking == 1) is the active one.
// PublicId (UUID v7) used in API routes.
public sealed class Package : AuditableEntity
{
    private readonly List<PackageLine> _lines = [];

    private Package() { }

    public Guid PublicId { get; private set; }
    public int? Version { get; private set; } // Optimistic concurrency
    public int SaleId { get; private set; }
    public int Ranking { get; private set; } // 1 = primary, 2+ = alternates
    public PackageStatus Status { get; private set; }
    public string Name { get; private set; } = string.Empty; // Unique within a sale (case-insensitive)
    [SensitiveData] public decimal GrossProfit { get; private set; } // HomeSalePrice - (HomeEstimatedCost + ProjectCostEstimatedCosts)
    [SensitiveData] public decimal CommissionableGrossProfit { get; private set; } // GrossProfit minus excluded items
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

    public void SetName(string name) => Name = name;

    public void SetPrimary() => Ranking = 1;

    public void SetNonPrimary(int ranking) => Ranking = ranking;

    public void FlagForTaxRecalculation() => MustRecalculateTaxes = true;

    public void ClearTaxRecalculationFlag() => MustRecalculateTaxes = false;

    public void SetCommissionableGrossProfit(decimal value) => CommissionableGrossProfit = value;

    public void AddLine(PackageLine line)
    {
        _lines.Add(line);

        if (line is HomeLine)
        {
            Raise(new HomeLineUpdatedDomainEvent { PackageId = Id, SaleId = SaleId });
        }
        else if (line is LandLine)
        {
            Raise(new LandLineUpdatedDomainEvent { PackageId = Id, SaleId = SaleId });
        }
        else if (line is CreditLine)
        {
            // GP unaffected (ShouldExcludeFromPricing = true), no domain event
        }
        else if (line is InsuranceLine)
        {
            // GP affected when !ShouldExcludeFromPricing, no domain event yet
        }
        else if (line is WarrantyLine)
        {
            // GP affected when !ShouldExcludeFromPricing, no domain event yet
        }
        else if (line is ProjectCostLine)
        {
            // GP affected when !ShouldExcludeFromPricing, no domain event yet
        }
        else if (line is TradeInLine)
        {
            // GP unaffected (ShouldExcludeFromPricing = true), no domain event
        }
        else if (line is TaxLine)
        {
            // GP affected when !ShouldExcludeFromPricing, no domain event
        }
        else if (line is SalesTeamLine)
        {
            // GP unaffected (metadata only), no domain event
        }
    }

    public void RemoveLine(PackageLine line)
    {
        _lines.Remove(line);
    }

    public void RemoveLinesByType(params string[] lineTypes)
    {
        _lines.RemoveAll(line => lineTypes.Contains(line.LineType));
    }

    // --- Typed line removal methods ---

    // Removes the single line of the given type (1:1 cardinality lines: Home, Land, Tax, Warranty, SalesTeam).
    public T? RemoveLine<T>() where T : PackageLine
    {
        var line = _lines
            .OfType<T>()
            .SingleOrDefault();

        if (line is not null)
        {
            _lines.Remove(line);

            if (line is HomeLine)
            {
                Raise(new HomeLineUpdatedDomainEvent { PackageId = Id, SaleId = SaleId });
            }
            else if (line is LandLine)
            {
                Raise(new LandLineUpdatedDomainEvent { PackageId = Id, SaleId = SaleId });
            }
        }

        return line;
    }

    // Filtered removal — Insurance lines are 1:many by type, so they need a predicate.
    public InsuranceLine? RemoveOutsideInsuranceLine()
    {
        var line = _lines
            .OfType<InsuranceLine>()
            .SingleOrDefault(l => l.Details?.InsuranceType == InsuranceType.Outside);

        if (line is not null)
        {
            _lines.Remove(line);
        }

        return line;
    }

    public InsuranceLine? RemoveHomeFirstInsuranceLine()
    {
        var line = _lines
            .OfType<InsuranceLine>()
            .SingleOrDefault(l => l.Details?.InsuranceType == InsuranceType.HomeFirst);

        if (line is not null)
        {
            _lines.Remove(line);
        }

        return line;
    }

    public CreditLine? RemoveDownPaymentLine()
    {
        var line = _lines
            .OfType<CreditLine>()
            .SingleOrDefault(l => l.IsDownPayment);

        if (line is not null)
        {
            _lines.Remove(line);
        }

        return line;
    }

    public CreditLine? RemoveConcessionLine()
    {
        var line = _lines
            .OfType<CreditLine>()
            .SingleOrDefault(l => l.IsConcession);

        if (line is not null)
        {
            _lines.Remove(line);
        }

        return line;
    }

    public ProjectCostLine? RemoveProjectCost(int categoryId, int itemId)
    {
        var line = _lines
            .OfType<ProjectCostLine>()
            .SingleOrDefault(l =>
                l.Details?.CategoryId == categoryId
                && l.Details?.ItemId == itemId);

        if (line is not null)
        {
            _lines.Remove(line);
        }

        return line;
    }

    public int RemoveProjectCostsByCategory(int categoryId)
    {
        var removed = _lines
            .RemoveAll(l => l is ProjectCostLine pc && pc.Details?.CategoryId == categoryId);

        return removed;
    }

    public int RemoveAllProjectCosts(int categoryId, int itemId)
    {
        var removed = _lines
            .RemoveAll(l =>
                l is ProjectCostLine pc
                && pc.Details?.CategoryId == categoryId
                && pc.Details?.ItemId == itemId);

        return removed;
    }

    // Caller is responsible for calling this once after all line mutations are complete,
    // right before SaveChangesAsync. Matches legacy pattern where GP was a computed property
    // evaluated at persistence time, not after every individual mutation.
    public void RecalculateGrossProfit()
    {
        GrossProfit = _lines
            .Where(l => !l.ShouldExcludeFromPricing)
            .Sum(l => l.SalePrice - l.EstimatedCost);
    }
}
