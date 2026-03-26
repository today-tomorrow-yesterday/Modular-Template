using Rtl.Core.Application.EventBus;

namespace Modules.Sales.IntegrationEvents;

// Published to Inventory module when sale summary data changes.
[EventDetailType("rtl.sales.saleSummaryChanged")]
public sealed record SaleSummaryChangedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    string? StockNumber,
    Guid? SalePublicId,
    string? CustomerName,
    DateTime? ReceivedInDate,
    decimal? OriginalRetailPrice,
    decimal? CurrentRetailPrice,
    DateTime? UpdatedAt) : IntegrationEvent(Id, OccurredOnUtc);
