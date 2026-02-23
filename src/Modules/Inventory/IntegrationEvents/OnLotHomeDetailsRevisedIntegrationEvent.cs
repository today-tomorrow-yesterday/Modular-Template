using Rtl.Core.Application.EventBus;

namespace Modules.Inventory.IntegrationEvents;

// ECST — non-price, non-availability fields changed (specs, serial number,
// facility, etc.). Catch-all for property changes not covered by the
// specific PriceRevised or AvailabilityChanged events.
public sealed record OnLotHomeDetailsRevisedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    int OnLotHomeId,
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
