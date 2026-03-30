using Rtl.Core.Application.EventBus;

namespace Modules.Inventory.IntegrationEvents;

// ECST — retail/invoice/original price fields changed. Consumers upsert cache
// and may trigger pricing review on affected downstream entities.
[EventDetailType("rtl.inventory.onLotHomePriceRevised")]
public sealed record OnLotHomePriceRevisedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid PublicOnLotHomeId,
    int HomeCenterNumber,
    string StockNumber,
    string? StockType,
    string? Condition,
    string? BuildType,
    decimal? Width,
    decimal? Length,
    int? NumberOfBedrooms,
    int? NumberOfBathrooms,
    int? ModelYear,
    string? Model,
    string? Make,
    string? Facility,
    string? SerialNumber,
    decimal? TotalInvoiceAmount,
    decimal? OriginalRetailPrice,
    decimal? CurrentRetailPrice) : IntegrationEvent(Id, OccurredOnUtc);
