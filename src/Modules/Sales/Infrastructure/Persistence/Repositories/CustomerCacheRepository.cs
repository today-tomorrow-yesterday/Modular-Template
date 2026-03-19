using Microsoft.EntityFrameworkCore;
using Modules.Sales.Domain.CustomersCache;
using Rtl.Core.Infrastructure.Caching;

namespace Modules.Sales.Infrastructure.Persistence.Repositories;

internal sealed class CustomerCacheRepository(SalesDbContext dbContext)
    : CacheReadRepository<CustomerCache, int, SalesDbContext>(dbContext),
      ICustomerCacheRepository,
      ICustomerCacheWriter
{
    public async Task<CustomerCache?> GetByRefPublicIdAsync(Guid refPublicId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.RefPublicId == refPublicId, cancellationToken);
    }

    public async Task UpsertAsync(
        CustomerCache customerCache,
        CancellationToken cancellationToken = default)
    {
        var existing = await DbSet
            .FirstOrDefaultAsync(c => c.RefPublicId == customerCache.RefPublicId, cancellationToken);

        if (existing is null)
        {
            DbSet.Add(customerCache);
        }
        else
        {
            existing.RefPublicId = customerCache.RefPublicId;
            existing.LifecycleStage = customerCache.LifecycleStage;
            existing.HomeCenterNumber = customerCache.HomeCenterNumber;
            existing.DisplayName = customerCache.DisplayName;
            existing.SalesforceAccountId = customerCache.SalesforceAccountId;
            existing.LastSyncedAtUtc = customerCache.LastSyncedAtUtc;
            existing.FirstName = customerCache.FirstName;
            existing.MiddleName = customerCache.MiddleName;
            existing.LastName = customerCache.LastName;
            existing.Email = customerCache.Email;
            existing.Phone = customerCache.Phone;
            existing.CoBuyerFirstName = customerCache.CoBuyerFirstName;
            existing.CoBuyerLastName = customerCache.CoBuyerLastName;
            existing.PrimarySalesPersonFederatedId = customerCache.PrimarySalesPersonFederatedId;
            existing.PrimarySalesPersonFirstName = customerCache.PrimarySalesPersonFirstName;
            existing.PrimarySalesPersonLastName = customerCache.PrimarySalesPersonLastName;
            existing.SecondarySalesPersonFederatedId = customerCache.SecondarySalesPersonFederatedId;
            existing.SecondarySalesPersonFirstName = customerCache.SecondarySalesPersonFirstName;
            existing.SecondarySalesPersonLastName = customerCache.SecondarySalesPersonLastName;
        }

        await DbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateLifecycleStageAsync(
        Guid refPublicId,
        LifecycleStage newLifecycleStage,
        DateTime lastSyncedAtUtc,
        CancellationToken cancellationToken = default)
    {
        var existing = await DbSet
            .FirstOrDefaultAsync(c => c.RefPublicId == refPublicId, cancellationToken);

        if (existing is null) return;

        existing.LifecycleStage = newLifecycleStage;
        existing.LastSyncedAtUtc = lastSyncedAtUtc;

        await DbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateHomeCenterNumberAsync(
        Guid refPublicId,
        int newHomeCenterNumber,
        DateTime lastSyncedAtUtc,
        CancellationToken cancellationToken = default)
    {
        var existing = await DbSet
            .FirstOrDefaultAsync(c => c.RefPublicId == refPublicId, cancellationToken);

        if (existing is null) return;

        existing.HomeCenterNumber = newHomeCenterNumber;
        existing.LastSyncedAtUtc = lastSyncedAtUtc;

        await DbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateNameAsync(
        Guid refPublicId,
        string displayName,
        string? firstName,
        string? middleName,
        string? lastName,
        DateTime lastSyncedAtUtc,
        CancellationToken cancellationToken = default)
    {
        var existing = await DbSet
            .FirstOrDefaultAsync(c => c.RefPublicId == refPublicId, cancellationToken);

        if (existing is null) return;

        existing.DisplayName = displayName;
        existing.FirstName = firstName ?? string.Empty;
        existing.MiddleName = middleName;
        existing.LastName = lastName ?? string.Empty;
        existing.LastSyncedAtUtc = lastSyncedAtUtc;

        await DbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateContactPointsAsync(
        Guid refPublicId,
        string? email,
        string? phone,
        DateTime lastSyncedAtUtc,
        CancellationToken cancellationToken = default)
    {
        var existing = await DbSet
            .FirstOrDefaultAsync(c => c.RefPublicId == refPublicId, cancellationToken);

        if (existing is null) return;

        existing.Email = email;
        existing.Phone = phone;
        existing.LastSyncedAtUtc = lastSyncedAtUtc;

        await DbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateSalesAssignmentsAsync(
        Guid refPublicId,
        string? primaryFederatedId,
        string? primaryFirstName,
        string? primaryLastName,
        string? secondaryFederatedId,
        string? secondaryFirstName,
        string? secondaryLastName,
        DateTime lastSyncedAtUtc,
        CancellationToken cancellationToken = default)
    {
        var existing = await DbSet
            .FirstOrDefaultAsync(c => c.RefPublicId == refPublicId, cancellationToken);

        if (existing is null) return;

        existing.PrimarySalesPersonFederatedId = primaryFederatedId;
        existing.PrimarySalesPersonFirstName = primaryFirstName;
        existing.PrimarySalesPersonLastName = primaryLastName;
        existing.SecondarySalesPersonFederatedId = secondaryFederatedId;
        existing.SecondarySalesPersonFirstName = secondaryFirstName;
        existing.SecondarySalesPersonLastName = secondaryLastName;
        existing.LastSyncedAtUtc = lastSyncedAtUtc;

        await DbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateCoBuyerAsync(
        Guid refPublicId,
        string? coBuyerFirstName,
        string? coBuyerLastName,
        DateTime lastSyncedAtUtc,
        CancellationToken cancellationToken = default)
    {
        var existing = await DbSet
            .FirstOrDefaultAsync(c => c.RefPublicId == refPublicId, cancellationToken);

        if (existing is null) return;

        existing.CoBuyerFirstName = coBuyerFirstName;
        existing.CoBuyerLastName = coBuyerLastName;
        existing.LastSyncedAtUtc = lastSyncedAtUtc;

        await DbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateMailingAddressAsync(
        Guid refPublicId,
        DateTime lastSyncedAtUtc,
        CancellationToken cancellationToken = default)
    {
        // No mailing address columns cached currently — update sync timestamp only.
        var existing = await DbSet
            .FirstOrDefaultAsync(c => c.RefPublicId == refPublicId, cancellationToken);

        if (existing is null) return;

        existing.LastSyncedAtUtc = lastSyncedAtUtc;

        await DbContext.SaveChangesAsync(cancellationToken);
    }
}
