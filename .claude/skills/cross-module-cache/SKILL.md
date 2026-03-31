---
name: scaffold-cross-module-cache
description: Use when adding a new cache projection that receives data from another module via integration events - creates cache entity, EF config, writer interface, repository, and event handler
---

# Scaffold Cross-Module Cache Projection

Creates a cache entity that stores a read-only copy of another module's data, maintained via integration events. Cache projections are read replicas used for local queries and FK references.

## What You Create

For caching `{SourceEntity}` from `{SourceModule}` module into `{TargetModule}` (e.g., caching `Product` from `SampleSales` into `SampleOrders`, or `Order` from `SampleOrders` into `SampleSales`):

### 1. Cache Entity

**File:** `src/Modules/{TargetModule}/Domain/{Feature}Cache/{SourceEntity}Cache.cs`

**Simple read-only cache (most common):**
```csharp
using Rtl.Core.Domain.Caching;

namespace Modules.{TargetModule}.Domain.{Feature}Cache;

public sealed class {SourceEntity}Cache : ICacheProjection
{
    public int Id { get; set; }
    public Guid RefPublicId { get; set; }           // Source entity's PublicId — upsert key
    // ... cached fields with { get; set; }
    // Mark financial/PII fields with [SensitiveData]
    public DateTime LastSyncedAtUtc { get; set; }
}
```

**With domain event support (extends Entity — use when the cache needs to detect changes):**
```csharp
using Rtl.Core.Domain.Caching;
using Rtl.Core.Domain.Entities;

namespace Modules.{TargetModule}.Domain.{Feature}Cache;

public sealed class {SourceEntity}Cache : Entity, ICacheProjection
{
    public Guid RefPublicId { get; set; }
    // ... cached fields
    public DateTime LastSyncedAtUtc { get; set; }

    public void ApplyChangesFrom({SourceEntity}Cache incoming)
    {
        var significantChange = /* compare key fields */;
        // Apply all fields...
        LastSyncedAtUtc = incoming.LastSyncedAtUtc;

        if (significantChange)
        {
            Raise(new {SourceEntity}CacheChangedDomainEvent());
        }
    }
}
```

**Key rules:**
- Cross-module reference: `Guid RefPublicId` (short form)
- Additional cross-module references: `Guid RefPublic{EntityName}Id` (e.g., `RefPublicCustomerId` on OrderCache)
- Extend `Entity` only if you need domain events on the cache (change detection)
- Simple caches implement only `ICacheProjection` with public getters/setters

### 2. Writer Interface

**File:** `src/Modules/{TargetModule}/Domain/{Feature}Cache/I{SourceEntity}CacheWriter.cs`
```csharp
namespace Modules.{TargetModule}.Domain.{Feature}Cache;

public interface I{SourceEntity}CacheWriter
{
    Task UpsertAsync({SourceEntity}Cache cache, CancellationToken cancellationToken = default);
}
```

### 3. EF Configuration

**File:** `src/Modules/{TargetModule}/Infrastructure/Persistence/Configurations/{SourceEntity}CacheConfiguration.cs`

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

        builder.HasIndex(e => e.RefPublicId).IsUnique()
            .HasDatabaseName("ix_{table}_cache_ref_public_id");
    }
}
```

### 4. Repository Implementation

**File:** `src/Modules/{TargetModule}/Infrastructure/Persistence/Repositories/{SourceEntity}CacheRepository.cs`

```csharp
internal sealed class {SourceEntity}CacheRepository({TargetModule}DbContext dbContext)
    : CacheReadRepository<{SourceEntity}Cache, {TargetModule}DbContext>(dbContext),
      I{SourceEntity}CacheRepository,
      I{SourceEntity}CacheWriter
{
    public async Task UpsertAsync({SourceEntity}Cache cache, CancellationToken cancellationToken = default)
    {
        var existing = await DbSet
            .FirstOrDefaultAsync(e => e.RefPublicId == cache.RefPublicId, cancellationToken);

        if (existing is null)
        {
            DbSet.Add(cache);
        }
        else
        {
            existing.{Field1} = cache.{Field1};
            existing.{Field2} = cache.{Field2};
            // ... update all cached fields
            existing.LastSyncedAtUtc = cache.LastSyncedAtUtc;
        }

        await DbContext.SaveChangesAsync(cancellationToken);
    }
}
```

### 5. Integration Event Handler

**File:** `src/Modules/{TargetModule}/Presentation/IntegrationEvents/{SourceModule}/{SourceEntity}{Action}IntegrationEventHandler.cs`

```csharp
internal sealed class {SourceEntity}{Action}IntegrationEventHandler(
    I{SourceEntity}CacheWriter cacheWriter,
    ILogger<{SourceEntity}{Action}IntegrationEventHandler> logger)
    : IntegrationEventHandler<{SourceEntity}{Action}IntegrationEvent>
{
    public override async Task HandleAsync(
        {SourceEntity}{Action}IntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing {SourceEntity}{Action}: Public{SourceEntity}Id={Id}",
            integrationEvent.Public{SourceEntity}Id);

        var cache = new {SourceEntity}Cache
        {
            RefPublicId = integrationEvent.Public{SourceEntity}Id,
            // ... map fields from event
            LastSyncedAtUtc = DateTime.UtcNow
        };

        await cacheWriter.UpsertAsync(cache, cancellationToken);
    }
}
```

### 6. Wire Up

- Add `DbSet<{SourceEntity}Cache>` to `{TargetModule}DbContext`
- Add configuration to `OnModelCreating`
- Register `I{SourceEntity}CacheWriter` → `{SourceEntity}CacheRepository` in `{TargetModule}Module`
- Register `I{SourceEntity}CacheRepository` → `{SourceEntity}CacheRepository` for reads
- Generate migration

## Real Examples

**ProductCache** in SampleOrders (caches Product from SampleSales):
- `src/Modules/SampleOrders/Domain/ProductsCache/ProductCache.cs`
- `src/Modules/SampleOrders/Domain/ProductsCache/IProductCacheWriter.cs`

**OrderCache** in SampleSales (caches Order from SampleOrders):
- `src/Modules/SampleSales/Domain/OrdersCache/OrderCache.cs`
- `src/Modules/SampleSales/Domain/OrdersCache/IOrderCacheWriter.cs`

## Checklist

- [ ] Cache entity uses `Guid RefPublicId` (short form for primary reference)
- [ ] `int Id` is `UseIdentityAlwaysColumn` (target module owns this key)
- [ ] Table in `Schemas.Cache` schema
- [ ] Upsert matches on `RefPublicId`
- [ ] Event handler maps `e.Public{Entity}Id` → `cache.RefPublicId`
- [ ] Writer interface in Domain layer, implementation in Infrastructure
- [ ] Registered in DbContext, module DI, migration generated
