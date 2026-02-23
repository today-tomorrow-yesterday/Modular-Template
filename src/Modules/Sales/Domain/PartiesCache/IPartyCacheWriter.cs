namespace Modules.Sales.Domain.PartiesCache;

public interface IPartyCacheWriter
{
    Task UpsertAsync(
        PartyCache partyCache,
        PartyPersonCache? personCache,
        PartyOrganizationCache? organizationCache,
        CancellationToken cancellationToken = default);

    Task UpdateLifecycleStageAsync(
        int refPartyId,
        LifecycleStage newLifecycleStage,
        DateTime lastSyncedAtUtc,
        CancellationToken cancellationToken = default);

    Task UpdateHomeCenterNumberAsync(
        int refPartyId,
        int newHomeCenterNumber,
        DateTime lastSyncedAtUtc,
        CancellationToken cancellationToken = default);

    Task UpdateNameAsync(
        int refPartyId,
        PartyType partyType,
        string displayName,
        string? firstName,
        string? middleName,
        string? lastName,
        string? organizationName,
        DateTime lastSyncedAtUtc,
        CancellationToken cancellationToken = default);

    Task UpdateContactPointsAsync(
        int refPartyId,
        string? email,
        string? phone,
        DateTime lastSyncedAtUtc,
        CancellationToken cancellationToken = default);

    Task UpdateSalesAssignmentsAsync(
        int refPartyId,
        string? primaryFederatedId,
        string? primaryFirstName,
        string? primaryLastName,
        string? secondaryFederatedId,
        string? secondaryFirstName,
        string? secondaryLastName,
        DateTime lastSyncedAtUtc,
        CancellationToken cancellationToken = default);

    Task UpdateCoBuyerAsync(
        int refPartyId,
        string? coBuyerFirstName,
        string? coBuyerLastName,
        DateTime lastSyncedAtUtc,
        CancellationToken cancellationToken = default);

    Task UpdateMailingAddressAsync(
        int refPartyId,
        DateTime lastSyncedAtUtc,
        CancellationToken cancellationToken = default);
}
