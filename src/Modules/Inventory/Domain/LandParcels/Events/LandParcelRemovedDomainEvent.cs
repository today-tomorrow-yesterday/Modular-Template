using Rtl.Core.Domain.Events;

namespace Modules.Inventory.Domain.LandParcels.Events;

public sealed record LandParcelRemovedDomainEvent(int HomeCenterNumber, string StockNumber) : DomainEvent;
