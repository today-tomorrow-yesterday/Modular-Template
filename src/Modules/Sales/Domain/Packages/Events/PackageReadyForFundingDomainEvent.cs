using Rtl.Core.Domain.Events;

namespace Modules.Sales.Domain.Packages.Events;

// Integration event → Funding module (ECST shape — fat data).
// Carries all data Funding needs to create a FundingRequest without calling back to Sales.
public sealed record PackageReadyForFundingDomainEvent : DomainEvent
{
    public int SaleId { get; init; }
    public Guid SalePublicId { get; init; }
    public int PackageId { get; init; }
    public Guid PackagePublicId { get; init; }
    public int HomeCenterNumber { get; init; }
    public string RetailLocationType { get; init; } = string.Empty;
    public string? StockNumber { get; init; }
    public decimal RequestAmount { get; init; }
}
