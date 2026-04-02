using ModularTemplate.Domain.Events;

namespace Modules.SampleSales.Domain.Catalogs.Events;

public sealed record CatalogProductAddedDomainEvent(
    Guid PublicProductId) : DomainEvent;
