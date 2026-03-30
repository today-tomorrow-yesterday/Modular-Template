# Inventory PublicId & Integration Event Cleanup

## Problem

Inventory integration events leak private `int Id` (iSeries CDC key) across module boundaries. The convention established by the Customer module is that cross-module references use `Guid Public{EntityName}Id` — never integer database keys. Currently all 8 Inventory integration events send `int OnLotHomeId` or `int LandParcelId`, and the Sales cache stores these ints as `RefOnLotHomeId` / `RefLandParcelId`.

## Scope

Only `OnLotHome` and `LandParcel` entities and their downstream consumers. No other modules or entities are in scope.

## Design

### 1. Inventory Domain — Add `Guid PublicId`

Add `Guid PublicId { get; private set; }` to both `OnLotHome` and `LandParcel` entities. Generated once in the `Create()` factory method via `Guid.CreateVersion7()`, same pattern as `Customer.PublicId`.

EF configurations add a `public_id` column with a unique index.

**Files changed:**
- `Inventory/Domain/OnLotHomes/OnLotHome.cs` — add property, set in `Create()`
- `Inventory/Domain/LandParcels/LandParcel.cs` — add property, set in `Create()`
- `Inventory/Infrastructure/Persistence/Configurations/OnLotHomeConfiguration.cs` — add column + unique index
- `Inventory/Infrastructure/Persistence/Configurations/LandParcelConfiguration.cs` — add column + unique index

### 2. Inventory Domain Events — Carry PublicId

**Remove domain events** simplify to just `Guid PublicId` (the only identifier the consumer needs):

```csharp
// Before:
public sealed record OnLotHomeRemovedDomainEvent(int HomeCenterNumber, string StockNumber) : DomainEvent;
public sealed record LandParcelRemovedDomainEvent(int HomeCenterNumber, string StockNumber) : DomainEvent;

// After:
public sealed record OnLotHomeRemovedDomainEvent(Guid PublicId) : DomainEvent;
public sealed record LandParcelRemovedDomainEvent(Guid PublicId) : DomainEvent;
```

`MarkRemoved()` changes from `Raise(new XRemovedDomainEvent(RefHomeCenterNumber, RefStockNumber))` to `Raise(new XRemovedDomainEvent(PublicId))`.

**Add/Revise domain events** carry no payload today (handlers load the entity from DB to get full state). No changes needed — handlers will read `PublicId` from the loaded entity alongside the other properties they already read.

**Files changed:**
- `Inventory/Domain/OnLotHomes/Events/OnLotHomeRemovedDomainEvent.cs`
- `Inventory/Domain/LandParcels/Events/LandParcelRemovedDomainEvent.cs`
- `Inventory/Domain/OnLotHomes/OnLotHome.cs` — `MarkRemoved()` method
- `Inventory/Domain/LandParcels/LandParcel.cs` — `MarkRemoved()` method

### 3. Integration Events — Replace `int` with `Guid`

All 8 Inventory integration events: replace `int OnLotHomeId`/`int LandParcelId` with `Guid PublicOnLotHomeId`/`Guid PublicLandParcelId`.

**Remove events** become lean (just the Guid):

```csharp
public sealed record OnLotHomeRemovedFromInventoryIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid PublicOnLotHomeId) : IntegrationEvent(Id, OccurredOnUtc);

public sealed record LandParcelRemovedFromInventoryIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid PublicLandParcelId) : IntegrationEvent(Id, OccurredOnUtc);
```

**Add events** keep all state fields but swap the int for Guid:

```csharp
// Before: int OnLotHomeId, int HomeCenterNumber, string StockNumber, ...
// After:  Guid PublicOnLotHomeId, int HomeCenterNumber, string StockNumber, ...
```

**Revise events** same swap — `int` identity field becomes `Guid`.

**Files changed (all 8):**
- `Inventory/IntegrationEvents/OnLotHomeAddedToInventoryIntegrationEvent.cs`
- `Inventory/IntegrationEvents/OnLotHomeRemovedFromInventoryIntegrationEvent.cs`
- `Inventory/IntegrationEvents/OnLotHomePriceRevisedIntegrationEvent.cs`
- `Inventory/IntegrationEvents/OnLotHomeDetailsRevisedIntegrationEvent.cs`
- `Inventory/IntegrationEvents/LandParcelAddedToInventoryIntegrationEvent.cs`
- `Inventory/IntegrationEvents/LandParcelRemovedFromInventoryIntegrationEvent.cs`
- `Inventory/IntegrationEvents/LandParcelDetailsRevisedIntegrationEvent.cs`
- `Inventory/IntegrationEvents/LandParcelAppraisalRevisedIntegrationEvent.cs`

### 4. Inventory Domain Event Handlers — Map PublicId

All handlers that map domain events to integration events update to pass `PublicId` (Guid) instead of `EntityId` (int).

**Remove handlers** read `PublicId` from the domain event (which now carries it):

```csharp
new OnLotHomeRemovedFromInventoryIntegrationEvent(
    Guid.CreateVersion7(),
    dateTimeProvider.UtcNow,
    domainEvent.PublicId)  // was: domainEvent.EntityId
```

**Add/Revise handlers** load the full entity from the repository — they read `entity.PublicId` alongside all the other properties they already map.

**Files changed:**
- `Inventory/Application/OnLotInventory/EventHandlers/OnLotHomeRemovedDomainEventHandler.cs`
- `Inventory/Application/OnLotInventory/EventHandlers/OnLotHomeAddedDomainEventHandler.cs`
- `Inventory/Application/OnLotInventory/EventHandlers/OnLotHomePriceRevisedDomainEventHandler.cs`
- `Inventory/Application/OnLotInventory/EventHandlers/OnLotHomeDetailsRevisedDomainEventHandler.cs`
- `Inventory/Application/LandInventory/EventHandlers/LandParcelRemovedDomainEventHandler.cs`
- `Inventory/Application/LandInventory/EventHandlers/LandParcelAddedDomainEventHandler.cs`
- `Inventory/Application/LandInventory/EventHandlers/LandParcelDetailsRevisedDomainEventHandler.cs`
- `Inventory/Application/LandInventory/EventHandlers/LandParcelAppraisalRevisedDomainEventHandler.cs`

### 5. Sales Cache Entities — Replace `int Ref` with `Guid Ref`

```csharp
// Before:
public int RefOnLotHomeId { get; set; }
public int RefLandParcelId { get; set; }

// After:
public Guid RefPublicOnLotHomeId { get; set; }
public Guid RefPublicLandParcelId { get; set; }
```

EF configurations: rename column, change type, update unique index.

**Files changed:**
- `Sales/Domain/InventoryCache/OnLotHomeCache.cs`
- `Sales/Domain/InventoryCache/LandParcelCache.cs`
- `Sales/Infrastructure/Persistence/Configurations/OnLotHomeCacheConfiguration.cs`
- `Sales/Infrastructure/Persistence/Configurations/LandParcelCacheConfiguration.cs`

### 6. Sales Cache Interfaces — Rename Methods

```csharp
// IOnLotHomeCacheWriter
// Before: MarkAsRemovedByRefIdAsync(int refOnLotHomeId, ...)
// After:  MarkAsRemovedByPublicIdAsync(Guid publicOnLotHomeId, ...)

// ILandParcelCacheWriter
// Before: MarkAsRemovedByRefIdAsync(int refLandParcelId, ...)
// After:  MarkAsRemovedByPublicIdAsync(Guid publicLandParcelId, ...)

// IInventoryCacheQueries
// Before: GetPackageLinesForHomeByRefIdAsync(int refOnLotHomeId, ...)
// After:  GetPackageLinesForHomeByPublicIdAsync(Guid publicOnLotHomeId, ...)
// Before: GetPackageLinesForLandByRefIdAsync(int refLandParcelId, ...)
// After:  GetPackageLinesForLandByPublicIdAsync(Guid publicLandParcelId, ...)
```

**Files changed:**
- `Sales/Domain/InventoryCache/IOnLotHomeCacheWriter.cs`
- `Sales/Domain/InventoryCache/ILandParcelCacheWriter.cs`
- `Sales/Domain/InventoryCache/IInventoryCacheQueries.cs`

### 7. Sales Cache Implementations — Update Queries

Repository implementations update to query by the new Guid column instead of the int column.

**Files changed:**
- `Sales/Infrastructure/Persistence/Repositories/OnLotHomeCacheRepository.cs`
- `Sales/Infrastructure/Persistence/Repositories/LandParcelCacheRepository.cs`
- `Sales/Infrastructure/Persistence/Repositories/InventoryCacheQueries.cs`

### 8. Sales Commands & Handlers — Use Guid

**Remove commands:**

```csharp
// Before:
public sealed record RemoveOnLotHomeCacheCommand(int RefOnLotHomeId, int HomeCenterNumber, string StockNumber) : ICommand;
public sealed record RemoveLandParcelCacheCommand(int RefLandParcelId, int HomeCenterNumber, string StockNumber) : ICommand;

// After:
public sealed record RemoveOnLotHomeCacheCommand(Guid PublicOnLotHomeId) : ICommand;
public sealed record RemoveLandParcelCacheCommand(Guid PublicLandParcelId) : ICommand;
```

Handlers update to use `PublicOnLotHomeId`/`PublicLandParcelId` for lookup. StockNumber/HomeCenterNumber for logging come from the cache row after lookup.

**Add handler mappings** update `e.OnLotHomeId` → `e.PublicOnLotHomeId` when populating cache entities.

**Files changed:**
- `Sales/Application/InventoryCache/RemoveOnLotHomeCache/RemoveOnLotHomeCacheCommand.cs`
- `Sales/Application/InventoryCache/RemoveOnLotHomeCache/RemoveOnLotHomeCacheCommandHandler.cs`
- `Sales/Application/InventoryCache/RemoveLandParcelCache/RemoveLandParcelCacheCommand.cs`
- `Sales/Application/InventoryCache/RemoveLandParcelCache/RemoveLandParcelCacheCommandHandler.cs`
- `Sales/Presentation/IntegrationEvents/Inventory/OnLotHomeAddedToInventoryIntegrationEventHandler.cs`
- `Sales/Presentation/IntegrationEvents/Inventory/OnLotHomeRemovedFromInventoryIntegrationEventHandler.cs`
- `Sales/Presentation/IntegrationEvents/Inventory/LandParcelAddedToInventoryIntegrationEventHandler.cs`
- `Sales/Presentation/IntegrationEvents/Inventory/LandParcelRemovedFromInventoryIntegrationEventHandler.cs`

- `Sales/Presentation/IntegrationEvents/Inventory/OnLotHomePriceRevisedIntegrationEventHandler.cs`
- `Sales/Presentation/IntegrationEvents/Inventory/OnLotHomeDetailsRevisedIntegrationEventHandler.cs`
- `Sales/Presentation/IntegrationEvents/Inventory/LandParcelDetailsRevisedIntegrationEventHandler.cs`
- `Sales/Presentation/IntegrationEvents/Inventory/LandParcelAppraisalRevisedIntegrationEventHandler.cs`

### 9. EF Migrations

Regenerate migrations for both Inventory and Sales modules after the schema changes. Requires `ENCRYPTION_KEY` env var.

## Out of Scope

- Revise event Sales consumers (detail/price/appraisal handlers) — only update the identity field mapping, no behavioral changes
- Adding PublicId to any entities outside Inventory
- The `DeliveryAddressCreatedIntegrationEvent` int SaleId issue in Sales (separate concern)
- The "product claimed" flow (documented as unimplemented in handler comments)
