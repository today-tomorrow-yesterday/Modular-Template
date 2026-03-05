using Rtl.Core.Application.EventBus;

namespace Modules.SampleSales.IntegrationEvents;

/// <summary>
/// Integration event published when a product is updated.
/// Other modules (e.g., Orders) can subscribe to sync their ProductCache.
/// </summary>
[EventDetailType("rtl.sampleSales.productUpdated")]
public sealed record ProductUpdatedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    int ProductId,
    string Name,
    string? Description,
    decimal Price,
    bool IsActive)
    : IntegrationEvent(Id, OccurredOnUtc);
