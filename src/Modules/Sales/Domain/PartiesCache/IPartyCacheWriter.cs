namespace Modules.Sales.Domain.PartiesCache;

public interface IPartyCacheWriter
{
    Task UpsertAsync(
        PartyCache partyCache,
        PartyPersonCache? personCache,
        PartyOrganizationCache? organizationCache,
        CancellationToken cancellationToken = default);

    Task UpdateLifecycleStageAsync(
        Guid refPublicId,
        LifecycleStage newLifecycleStage,
        DateTime lastSyncedAtUtc,
        CancellationToken cancellationToken = default);

    Task UpdateHomeCenterNumberAsync(
        Guid refPublicId,
        int newHomeCenterNumber,
        DateTime lastSyncedAtUtc,
        CancellationToken cancellationToken = default);

    Task UpdateNameAsync(
        Guid refPublicId,
        PartyType partyType,
        string displayName,
        string? firstName,
        string? middleName,
        string? lastName,
        string? organizationName,
        DateTime lastSyncedAtUtc,
        CancellationToken cancellationToken = default);

    Task UpdateContactPointsAsync(
        Guid refPublicId,
        string? email,
        string? phone,
        DateTime lastSyncedAtUtc,
        CancellationToken cancellationToken = default);

    Task UpdateSalesAssignmentsAsync(
        Guid refPublicId,
        string? primaryFederatedId,
        string? primaryFirstName,
        string? primaryLastName,
        string? secondaryFederatedId,
        string? secondaryFirstName,
        string? secondaryLastName,
        DateTime lastSyncedAtUtc,
        CancellationToken cancellationToken = default);

    Task UpdateCoBuyerAsync(
        Guid refPublicId,
        string? coBuyerFirstName,
        string? coBuyerLastName,
        DateTime lastSyncedAtUtc,
        CancellationToken cancellationToken = default);

    Task UpdateMailingAddressAsync(
        Guid refPublicId,
        DateTime lastSyncedAtUtc,
        CancellationToken cancellationToken = default);
}
