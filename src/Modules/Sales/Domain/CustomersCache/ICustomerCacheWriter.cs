namespace Modules.Sales.Domain.CustomersCache;

public interface ICustomerCacheWriter
{
    Task UpsertAsync(CustomerCache customerCache, CancellationToken cancellationToken = default);
    Task UpdateLifecycleStageAsync(Guid refPublicId, LifecycleStage newLifecycleStage, DateTime lastSyncedAtUtc, CancellationToken cancellationToken = default);
    Task UpdateHomeCenterNumberAsync(Guid refPublicId, int newHomeCenterNumber, DateTime lastSyncedAtUtc, CancellationToken cancellationToken = default);
    Task UpdateNameAsync(Guid refPublicId, string displayName, string? firstName, string? middleName, string? lastName, DateTime lastSyncedAtUtc, CancellationToken cancellationToken = default);
    Task UpdateContactPointsAsync(Guid refPublicId, string? email, string? phone, DateTime lastSyncedAtUtc, CancellationToken cancellationToken = default);
    Task UpdateSalesAssignmentsAsync(Guid refPublicId, string? primaryFederatedId, string? primaryFirstName, string? primaryLastName, string? secondaryFederatedId, string? secondaryFirstName, string? secondaryLastName, DateTime lastSyncedAtUtc, CancellationToken cancellationToken = default);
    Task UpdateCoBuyerAsync(Guid refPublicId, string? coBuyerFirstName, string? coBuyerLastName, DateTime lastSyncedAtUtc, CancellationToken cancellationToken = default);
    Task UpdateMailingAddressAsync(Guid refPublicId, DateTime lastSyncedAtUtc, CancellationToken cancellationToken = default);
}
