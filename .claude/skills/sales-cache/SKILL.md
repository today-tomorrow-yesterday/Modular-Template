---
name: scaffold-sales-cache
description: Use when adding a new cache projection to the Sales module - creates cache entity, EF config, writer interface, repository, integration event handler, and upsert command
---

# Scaffold Sales Cache Projection

Creates a complete cache entity in the Sales module that receives data from another module's integration events. Cache projections are read replicas of external data used for local queries and FK references.

## What You Create

For caching `{SourceEntity}` from `{SourceModule}` module (e.g., caching `OnLotHome` from `Inventory`):

### 1. Cache Entity

**File:** `src/Modules/Sales/Domain/{Feature}Cache/{SourceEntity}Cache.cs`

**With domain event support (extends Entity):**
```csharp
using Rtl.Core.Domain.Auditing;
using Rtl.Core.Domain.Caching;
using Rtl.Core.Domain.Entities;

namespace Modules.Sales.Domain.{Feature}Cache;

public sealed class {SourceEntity}Cache : Entity, ICacheProjection
{
    public Guid RefPublicId { get; set; }        // Source entity's PublicId — upsert key
    public int RefHomeCenterNumber { get; set; }
    public string RefStockNumber { get; set; } = string.Empty;
    // ... cached fields with { get; set; }
    // Mark financial/PII fields with [SensitiveData]
    public DateTime LastSyncedAtUtc { get; set; }
    public bool IsRemovedFromInventory { get; private set; }

    public void MarkAsRemovedFromInventory()
    {
        IsRemovedFromInventory = true;
    }

    public void ApplyChangesFrom({SourceEntity}Cache incoming)
    {
        // Detect significant changes before applying
        var significantChange = /* compare key fields */;

        // Apply all fields
        RefHomeCenterNumber = incoming.RefHomeCenterNumber;
        RefStockNumber = incoming.RefStockNumber;
        // ... all other fields
        LastSyncedAtUtc = incoming.LastSyncedAtUtc;

        if (significantChange)
        {
            Raise(new {SourceEntity}Cache{Change}DomainEvent { /* minimal identity */ });
        }
    }
}
```

**Without domain event support (simple cache):**
```csharp
public sealed class {SourceEntity}Cache : ICacheProjection
{
    public int Id { get; set; }
    public Guid RefPublicId { get; set; }
    // ... fields with { get; set; }
    public DateTime LastSyncedAtUtc { get; set; }

    public void ApplyChangesFrom({SourceEntity}Cache incoming)
    {
        // Apply all fields, no event detection needed
    }
}
```

**Key rules:**
- Cross-module reference: `Guid RefPublicId` (short form, NOT `RefPublic{Entity}Id`)
- Extend `Entity` only if you need domain events (price/appraisal change detection)
- `IsRemovedFromInventory` flag for soft-delete on removal events
- `ApplyChangesFrom()` for upsert — detects significant changes and raises events

### 2. Writer Interface

**File:** `src/Modules/Sales/Domain/{Feature}Cache/I{SourceEntity}CacheWriter.cs`
```csharp
namespace Modules.Sales.Domain.{Feature}Cache;

public interface I{SourceEntity}CacheWriter
{
    Task UpsertAsync({SourceEntity}Cache cache, CancellationToken cancellationToken = default);
    Task MarkAsRemovedByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default);
}
```

### 3. EF Configuration

**File:** `src/Modules/Sales/Infrastructure/Persistence/Configurations/{SourceEntity}CacheConfiguration.cs`

```csharp
internal sealed class {SourceEntity}CacheConfiguration : IEntityTypeConfiguration<{SourceEntity}Cache>
{
    public void Configure(EntityTypeBuilder<{SourceEntity}Cache> builder)
    {
        builder.ToTable("{snake_case}_cache", Schemas.Cache);  // cache schema

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();

        builder.Property(e => e.RefPublicId)
            .HasColumnName("ref_public_id")
            .IsRequired();

        // ... other properties with snake_case column names

        builder.Property(e => e.LastSyncedAtUtc)
            .HasColumnName("last_synced_at_utc").IsRequired();

        builder.Property(e => e.IsRemovedFromInventory)
            .HasColumnName("is_removed_from_inventory")
            .HasDefaultValue(false).IsRequired();

        builder.HasIndex(e => e.RefPublicId).IsUnique()
            .HasDatabaseName("ix_{table}_cache_ref_public_id");

        builder.HasIndex(e => new { e.RefHomeCenterNumber, e.RefStockNumber }).IsUnique()
            .HasDatabaseName("ix_{table}_cache_hc_stock");
    }
}
```

### 4. Repository Implementation

**File:** `src/Modules/Sales/Infrastructure/Persistence/Repositories/{SourceEntity}CacheRepository.cs`

```csharp
internal sealed class {SourceEntity}CacheRepository(SalesDbContext dbContext)
    : CacheReadRepository<{SourceEntity}Cache, SalesDbContext>(dbContext),
      I{SourceEntity}CacheWriter
{
    public async Task UpsertAsync({SourceEntity}Cache cache, CancellationToken cancellationToken = default)
    {
        var existing = await DbSet
            .FirstOrDefaultAsync(e => e.RefPublicId == cache.RefPublicId, cancellationToken);

        if (existing is null)
            DbSet.Add(cache);
        else
            existing.ApplyChangesFrom(cache);

        await DbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAsRemovedByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default)
    {
        var existing = await DbSet
            .FirstOrDefaultAsync(e => e.RefPublicId == publicId, cancellationToken);

        if (existing is not null)
        {
            existing.MarkAsRemovedFromInventory();
            await DbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
```

### 5. Integration Event Handler (Added)

**File:** `src/Modules/Sales/Presentation/IntegrationEvents/{SourceModule}/{SourceEntity}AddedIntegrationEventHandler.cs`

```csharp
internal sealed class {SourceEntity}AddedIntegrationEventHandler(
    ISender sender,
    ILogger<{SourceEntity}AddedIntegrationEventHandler> logger)
    : IntegrationEventHandler<{SourceEntity}AddedToInventoryIntegrationEvent>
{
    public override async Task HandleAsync(
        {SourceEntity}AddedToInventoryIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing {SourceEntity}Added: Public{SourceEntity}Id={Id}, HC={HC}, Stock={Stock}",
            integrationEvent.Public{SourceEntity}Id,
            integrationEvent.HomeCenterNumber,
            integrationEvent.StockNumber);

        await sender.Send(
            new Create{SourceEntity}CacheCommand(MapToCache(integrationEvent)),
            cancellationToken);
    }

    private static {SourceEntity}Cache MapToCache({Event} e) => new()
    {
        RefPublicId = e.Public{SourceEntity}Id,    // Guid from event → RefPublicId on cache
        RefHomeCenterNumber = e.HomeCenterNumber,
        RefStockNumber = e.StockNumber,
        // ... map all fields
    };
}
```

### 6. Integration Event Handler (Removed)

**File:** `src/Modules/Sales/Presentation/IntegrationEvents/{SourceModule}/{SourceEntity}RemovedIntegrationEventHandler.cs`

```csharp
internal sealed class {SourceEntity}RemovedIntegrationEventHandler(
    ISender sender,
    ILogger<{SourceEntity}RemovedIntegrationEventHandler> logger)
    : IntegrationEventHandler<{SourceEntity}RemovedFromInventoryIntegrationEvent>
{
    public override async Task HandleAsync(
        {SourceEntity}RemovedFromInventoryIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogWarning(  // LogWarning for removals
            "Processing {SourceEntity}Removed: Public{SourceEntity}Id={Id}",
            integrationEvent.Public{SourceEntity}Id);

        await sender.Send(
            new Remove{SourceEntity}CacheCommand(integrationEvent.Public{SourceEntity}Id),
            cancellationToken);
    }
}
```

### 7. Commands

**Create/Upsert:** `Create{SourceEntity}CacheCommand({SourceEntity}Cache Cache) : ICommand`
**Remove:** `Remove{SourceEntity}CacheCommand(Guid Public{SourceEntity}Id) : ICommand`

Remove command takes ONLY the Guid — handler looks up StockNumber/HomeCenterNumber from the cache row for logging.

### 8. Wire Up

- Add `DbSet<{SourceEntity}Cache>` to `SalesDbContext`
- Add configuration to `OnModelCreating`
- Register `I{SourceEntity}CacheWriter` → `{SourceEntity}CacheRepository` in `SalesModule`
- Add read repository registration if needed
- Generate migration

## Checklist

- [ ] Cache entity uses `Guid RefPublicId` (short form)
- [ ] `int Id` is `UseIdentityAlwaysColumn` (Sales owns this key)
- [ ] Table in `Schemas.Cache` schema
- [ ] Upsert matches on `RefPublicId`
- [ ] Remove command takes only `Guid` — lean
- [ ] Event handler maps `e.Public{Entity}Id` → `cache.RefPublicId`
- [ ] Removals log at `LogWarning` level
- [ ] `IsRemovedFromInventory` soft-delete flag present
- [ ] Registered in DbContext, module DI, migration generated
