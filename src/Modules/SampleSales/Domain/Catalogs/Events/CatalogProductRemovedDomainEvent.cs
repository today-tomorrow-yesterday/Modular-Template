using ModularTemplate.Domain.Events;

namespace Modules.SampleSales.Domain.Catalogs.Events;

public sealed record CatalogProductRemovedDomainEvent(
    Guid PublicProductId) : DomainEvent;
