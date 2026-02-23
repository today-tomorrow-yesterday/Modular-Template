using Rtl.Core.Application.EventBus;

namespace Modules.Organization.IntegrationEvents;

// ECST event — full home center state for cache population.
// Consumers: Sales (sales.retail_locations), Inventory (cache.home_centers)
public sealed record HomeCenterChangedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    int HomeCenterId,
    Guid PublicId,
    int HomeCenterNumber,
    string LotName,
    string StateCode,
    string Zip,
    bool IsActive,
    int? ManagerEmployeeNumber,
    double? Latitude,
    double? Longitude,
    int? ZoneId,
    int? RegionId) : IntegrationEvent(Id, OccurredOnUtc);
