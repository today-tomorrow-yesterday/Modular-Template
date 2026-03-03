using Microsoft.EntityFrameworkCore;
using Modules.Sales.Domain.PartiesCache;
using Rtl.Core.Infrastructure.Caching;

namespace Modules.Sales.Infrastructure.Persistence.Repositories;

internal sealed class PartyCacheRepository(SalesDbContext dbContext)
    : CacheReadRepository<PartyCache, int, SalesDbContext>(dbContext),
      IPartyCacheRepository,
      IPartyCacheWriter
{
    public async Task<PartyCache?> GetByRefPublicIdAsync(Guid refPublicId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.RefPublicId == refPublicId, cancellationToken);
    }

    public async Task UpsertAsync(
        PartyCache partyCache,
        PartyPersonCache? personCache,
        PartyOrganizationCache? organizationCache,
        CancellationToken cancellationToken = default)
    {
        var existing = await DbSet
            .Include(p => p.Person)
            .Include(p => p.Organization)
            .FirstOrDefaultAsync(p => p.RefPublicId == partyCache.RefPublicId, cancellationToken);

        if (existing is null)
        {
            if (personCache is not null)
            {
                partyCache.Person = personCache;
            }
            else if (organizationCache is not null)
            {
                partyCache.Organization = organizationCache;
            }

            DbSet.Add(partyCache);
        }
        else
        {
            existing.RefPublicId = partyCache.RefPublicId;
            existing.PartyType = partyCache.PartyType;
            existing.LifecycleStage = partyCache.LifecycleStage;
            existing.HomeCenterNumber = partyCache.HomeCenterNumber;
            existing.DisplayName = partyCache.DisplayName;
            existing.SalesforceAccountId = partyCache.SalesforceAccountId;
            existing.LastSyncedAtUtc = partyCache.LastSyncedAtUtc;

            if (personCache is not null)
            {
                if (existing.Person is null)
                {
                    personCache.PartyId = existing.Id;
                    DbContext.Set<PartyPersonCache>().Add(personCache);
                }
                else
                {
                    existing.Person.FirstName = personCache.FirstName;
                    existing.Person.MiddleName = personCache.MiddleName;
                    existing.Person.LastName = personCache.LastName;
                    existing.Person.Email = personCache.Email;
                    existing.Person.Phone = personCache.Phone;
                    existing.Person.CoBuyerFirstName = personCache.CoBuyerFirstName;
                    existing.Person.CoBuyerLastName = personCache.CoBuyerLastName;
                    existing.Person.PrimarySalesPersonFederatedId = personCache.PrimarySalesPersonFederatedId;
                    existing.Person.PrimarySalesPersonFirstName = personCache.PrimarySalesPersonFirstName;
                    existing.Person.PrimarySalesPersonLastName = personCache.PrimarySalesPersonLastName;
                    existing.Person.SecondarySalesPersonFederatedId = personCache.SecondarySalesPersonFederatedId;
                    existing.Person.SecondarySalesPersonFirstName = personCache.SecondarySalesPersonFirstName;
                    existing.Person.SecondarySalesPersonLastName = personCache.SecondarySalesPersonLastName;
                }
            }
            else if (organizationCache is not null)
            {
                if (existing.Organization is null)
                {
                    organizationCache.PartyId = existing.Id;
                    DbContext.Set<PartyOrganizationCache>().Add(organizationCache);
                }
                else
                {
                    existing.Organization.OrganizationName = organizationCache.OrganizationName;
                }
            }
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
            .FirstOrDefaultAsync(p => p.RefPublicId == refPublicId, cancellationToken);

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
            .FirstOrDefaultAsync(p => p.RefPublicId == refPublicId, cancellationToken);

        if (existing is null) return;

        existing.HomeCenterNumber = newHomeCenterNumber;
        existing.LastSyncedAtUtc = lastSyncedAtUtc;

        await DbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateNameAsync(
        Guid refPublicId,
        PartyType partyType,
        string displayName,
        string? firstName,
        string? middleName,
        string? lastName,
        string? organizationName,
        DateTime lastSyncedAtUtc,
        CancellationToken cancellationToken = default)
    {
        var existing = await DbSet
            .Include(p => p.Person)
            .Include(p => p.Organization)
            .FirstOrDefaultAsync(p => p.RefPublicId == refPublicId, cancellationToken);

        if (existing is null) return;

        existing.DisplayName = displayName;
        existing.LastSyncedAtUtc = lastSyncedAtUtc;

        if (partyType == PartyType.Person && existing.Person is not null)
        {
            existing.Person.FirstName = firstName ?? string.Empty;
            existing.Person.MiddleName = middleName;
            existing.Person.LastName = lastName ?? string.Empty;
        }
        else if (partyType == PartyType.Organization && existing.Organization is not null)
        {
            existing.Organization.OrganizationName = organizationName ?? string.Empty;
        }

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
            .Include(p => p.Person)
            .FirstOrDefaultAsync(p => p.RefPublicId == refPublicId, cancellationToken);

        if (existing?.Person is null) return;

        existing.Person.Email = email;
        existing.Person.Phone = phone;
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
            .Include(p => p.Person)
            .FirstOrDefaultAsync(p => p.RefPublicId == refPublicId, cancellationToken);

        if (existing?.Person is null) return;

        existing.Person.PrimarySalesPersonFederatedId = primaryFederatedId;
        existing.Person.PrimarySalesPersonFirstName = primaryFirstName;
        existing.Person.PrimarySalesPersonLastName = primaryLastName;
        existing.Person.SecondarySalesPersonFederatedId = secondaryFederatedId;
        existing.Person.SecondarySalesPersonFirstName = secondaryFirstName;
        existing.Person.SecondarySalesPersonLastName = secondaryLastName;
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
            .Include(p => p.Person)
            .FirstOrDefaultAsync(p => p.RefPublicId == refPublicId, cancellationToken);

        if (existing?.Person is null) return;

        existing.Person.CoBuyerFirstName = coBuyerFirstName;
        existing.Person.CoBuyerLastName = coBuyerLastName;
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
            .FirstOrDefaultAsync(p => p.RefPublicId == refPublicId, cancellationToken);

        if (existing is null) return;

        existing.LastSyncedAtUtc = lastSyncedAtUtc;

        await DbContext.SaveChangesAsync(cancellationToken);
    }
}
