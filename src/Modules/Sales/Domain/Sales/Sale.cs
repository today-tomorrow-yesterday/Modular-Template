using Modules.Sales.Domain.DeliveryAddresses;
using Modules.Sales.Domain.FundingCache;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.PartiesCache;
using Modules.Sales.Domain.RetailLocations;
using Modules.Sales.Domain.Sales.Events;
using Rtl.Core.Domain.Entities;

namespace Modules.Sales.Domain.Sales;

// Aggregate root — sales.sales. Tracks the lifecycle of a manufactured home sale.
public sealed class Sale : SoftDeletableEntity, IAggregateRoot
{
    private readonly List<Package> _packages = [];

    private Sale() { }

    public Guid PublicId { get; private set; }
    public int PartyId { get; private set; }
    public int RetailLocationId { get; private set; }
    public SaleType SaleType { get; private set; }
    public SaleStatus SaleStatus { get; private set; }
    public int SaleNumber { get; private set; }

    public PartyCache Party { get; private set; } = null!;
    public RetailLocation RetailLocation { get; private set; } = null!;
    public DeliveryAddress? DeliveryAddress { get; private set; }
    public IReadOnlyCollection<Package> Packages => _packages.AsReadOnly();
    public ICollection<FundingRequestCache> FundingRequests { get; private set; } = [];

    public static Sale Create(
        int partyId,
        int retailLocationId,
        SaleType saleType,
        int saleNumber)
    {
        var sale = new Sale
        {
            PublicId = Guid.CreateVersion7(),
            PartyId = partyId,
            RetailLocationId = retailLocationId,
            SaleType = saleType,
            SaleStatus = SaleStatus.Inquiry,
            SaleNumber = saleNumber
        };

        sale.Raise(new SaleSummaryChangedDomainEvent { SaleId = sale.Id });

        return sale;
    }

    public void UpdateStatus(SaleStatus newStatus)
    {
        SaleStatus = newStatus;
        Raise(new SaleSummaryChangedDomainEvent { SaleId = Id });
    }

    public void RaiseSaleSummaryChanged()
    {
        Raise(new SaleSummaryChangedDomainEvent { SaleId = Id });
    }
}
