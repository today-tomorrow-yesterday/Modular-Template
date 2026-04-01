# Auditing

Three auditing concepts implemented via EF Core interceptors.

## Entity Hierarchy

```
Entity                    → Domain events only
    └── AuditableEntity   → + Created/Modified tracking
            └── SoftDeletableEntity  → + Soft delete tracking
```

## 1. Auditable Entities

**What**: Tracks who created/modified an entity and when.

**Interceptor**: `AuditableEntitiesInterceptor`

**Fields** (auto-populated on SaveChanges):
| Field | Set On |
|-------|--------|
| `CreatedByUserId` | Insert |
| `CreatedAtUtc` | Insert |
| `ModifiedByUserId` | Insert, Update |
| `ModifiedAtUtc` | Insert, Update |

**Usage**: Inherit from `AuditableEntity`
```csharp
public sealed class Customer : AuditableEntity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
}
```

## 2. Soft Deletable Entities

**What**: Converts DELETE to UPDATE with `IsDeleted = true`.

**Interceptor**: `SoftDeleteInterceptor`

**Fields** (auto-populated on delete):
| Field | Description |
|-------|-------------|
| `IsDeleted` | True when soft-deleted |
| `DeletedAtUtc` | Timestamp of deletion |
| `DeletedByUserId` | Who deleted it |

**Usage**: Inherit from `SoftDeletableEntity`
```csharp
public sealed class Order : SoftDeletableEntity
{
    public Guid Id { get; private set; }
    // Inherits all audit + soft delete fields
}
```

**Query Filter**: Soft-deleted entities are automatically excluded via EF Core global filter.

**Restore**: Call `entity.Restore()` to undo soft delete.

## 3. Audit Trail

**What**: Captures field-level change history to `AuditLog` table.

**Interceptor**: `AuditTrailInterceptor`

**Tracked Actions**:
- `Insert` - New entity created
- `Update` - Entity modified
- `Delete` - Entity hard-deleted
- `SoftDelete` - Entity soft-deleted
- `Restore` - Entity restored from soft delete

**What's Logged**:
| Field | Description |
|-------|-------------|
| `EntityName` | Table/entity name |
| `EntityId` | Primary key value |
| `Action` | Insert/Update/Delete/SoftDelete/Restore |
| `OldValues` | Previous field values (JSON) |
| `NewValues` | New field values (JSON) |
| `AffectedColumns` | Which columns changed |
| `UserId` | Who made the change |
| `TimestampUtc` | When it happened |
| `CorrelationId` | Request correlation ID |
| `TraceId` | Distributed trace ID |

**Usage**: Implement `IAuditable` marker interface
```csharp
public sealed class Payment : SoftDeletableEntity, IAuditable
{
    // Full audit trail will be captured
}
```

## Interceptor Registration

Interceptors are registered in `ModuleDbContext`:
```csharp
optionsBuilder.AddInterceptors(
    serviceProvider.GetRequiredService<AuditableEntitiesInterceptor>(),
    serviceProvider.GetRequiredService<SoftDeleteInterceptor>(),
    serviceProvider.GetRequiredService<AuditTrailInterceptor>()
);
```

## Choosing the Right Base Class

| Need | Use |
|------|-----|
| Domain events only | `Entity` |
| + Created/Modified tracking | `AuditableEntity` |
| + Soft delete | `SoftDeletableEntity` |
| + Full change history | `SoftDeletableEntity` + `IAuditable` |
