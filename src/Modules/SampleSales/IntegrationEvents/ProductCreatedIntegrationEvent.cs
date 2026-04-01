using ModularTemplate.Application.EventBus;

namespace Modules.SampleSales.IntegrationEvents;

/// <summary>
/// Integration event published when a new product is created.
/// Other modules (e.g., Orders) can subscribe to sync their ProductCache.
/// </summary>
[EventDetailType("mt.sampleSales.productCreated")]
public sealed record ProductCreatedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid PublicProductId,
    string Name,
    string? Description,
    decimal Price)
    : IntegrationEvent(Id, OccurredOnUtc);
