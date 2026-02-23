# Audit Trail Implementation Plan

## Document Info
- **Created:** 2026-01-26
- **Status:** Ready for Implementation
- **Priority:** High

---

## Table of Contents
1. [Executive Summary](#executive-summary)
2. [Current State Analysis](#current-state-analysis)
3. [Recommended Architecture](#recommended-architecture)
4. [Implementation Plan](#implementation-plan)
5. [Phase Details](#phase-details)
6. [Code Examples](#code-examples)
7. [Migration Strategy](#migration-strategy)
8. [Testing Strategy](#testing-strategy)

---

## Executive Summary

### Goal
Implement field-level audit trail tracking to capture **what changed**, **when**, **by whom**, and provide context for debugging and compliance.

### Key Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Storage | Centralized `AuditLog` table with JSON columns | Flexibility, query capability, separation of concerns |
| Timing | Synchronous (same transaction) | ACID compliance, guaranteed capture |
| Detection | EF Core ChangeTracker | Native to stack, accurate change detection |
| Data | Changed fields only + context enrichment | Optimal storage, full debugging capability |
| Masking | Hybrid (convention + attributes) | Safe defaults with explicit overrides |
| Soft Delete | Detect IsDeleted transitions | Distinguish SoftDelete/Restore from Update |

### Scope
- Field-level change tracking for all `AuditableEntity` subclasses
- Centralized audit log table per module schema
- Correlation ID support for cross-request tracking
- Sensitive data masking
- Soft delete detection

### Out of Scope (Future Phases)
- Non-blocking async publishing
- HTTP activity auditing
- Security event auditing (login/logout)
- Exception event auditing

---

## Current State Analysis

### Existing Infrastructure

**Location:** `src/Common/Rtl.Core.Common.Infrastructure/Auditing/`

| File | Purpose | Status |
|------|---------|--------|
| `AuditableEntitiesInterceptor.cs` | Populates CreatedAt/ModifiedAt fields | Keep, extend |
| `SoftDeleteInterceptor.cs` | Converts hard deletes to soft deletes | Keep as-is |
| `AuditableEntityConfiguration.cs` | EF Core column mappings | Keep as-is |
| `SoftDeletableEntityConfiguration.cs` | Soft delete column mappings | Keep as-is |

**Current Audit Fields (embedded in entities):**
```csharp
// AuditableEntity.cs
public Guid CreatedByUserId { get; internal set; }
public DateTime CreatedAtUtc { get; internal set; }
public Guid? ModifiedByUserId { get; internal set; }
public DateTime? ModifiedAtUtc { get; internal set; }

// SoftDeletableEntity.cs (extends AuditableEntity)
public bool IsDeleted { get; internal set; }
public DateTime? DeletedAtUtc { get; internal set; }
public Guid? DeletedByUserId { get; internal set; }
```

**Gap:** No field-level change tracking. Cannot answer "what specifically changed?"

### TODO Found
```csharp
// ModuleDbContext.cs line 29
//TODO: add audit trails
//modelBuilder.ApplyAuditConfigurations();
```

---

## Recommended Architecture

### High-Level Flow

```
Entity Change
    │
    ▼
SaveChangesAsync() called
    │
    ▼
AuditTrailInterceptor.SavingChangesAsync()
    │
    ├── 1. Query ChangeTracker for Added/Modified/Deleted entities
    ├── 2. Build AuditEntry for each entity (capture old/new values)
    ├── 3. Detect soft delete transitions
    ├── 4. Mask sensitive fields
    ├── 5. Enrich with correlation context
    └── 6. Add AuditLog entities to context (same transaction)
    │
    ▼
base.SavingChangesAsync() executes
    │
    ▼
Business entities + AuditLog entities saved atomically
```

### Data Model

```
┌─────────────────────────────────────────────────────────────┐
│                        audit_logs                            │
├─────────────────────────────────────────────────────────────┤
│ id                  UUID PRIMARY KEY                         │
│ entity_name         VARCHAR(256) NOT NULL     -- "Product"   │
│ entity_id           VARCHAR(256) NOT NULL     -- "guid-here" │
│ action              INTEGER NOT NULL          -- 1,2,3,4,5   │
│ old_values          JSONB                     -- {"Price":9} │
│ new_values          JSONB                     -- {"Price":12}│
│ affected_columns    JSONB                     -- ["Price"]   │
│ user_id             UUID NOT NULL                            │
│ user_name           VARCHAR(256)                             │
│ timestamp_utc       TIMESTAMP NOT NULL                       │
│ correlation_id      VARCHAR(256)              -- Request ID  │
│ trace_id            VARCHAR(256)              -- W3C Trace   │
│ ip_address          VARCHAR(45)               -- IPv4/IPv6   │
│ user_agent          VARCHAR(512)                             │
├─────────────────────────────────────────────────────────────┤
│ INDEX ix_audit_logs_entity (entity_name, entity_id)          │
│ INDEX ix_audit_logs_user (user_id)                           │
│ INDEX ix_audit_logs_timestamp (timestamp_utc)                │
│ INDEX ix_audit_logs_correlation (correlation_id)             │
└─────────────────────────────────────────────────────────────┘
```

### Enum Definition

```csharp
public enum AuditAction
{
    Unknown = 0,
    Insert = 1,
    Update = 2,
    Delete = 3,      // Hard delete
    SoftDelete = 4,  // IsDeleted: false → true
    Restore = 5      // IsDeleted: true → false
}
```

---

## Implementation Plan

### Phase Overview

| Phase | Description | Effort | Dependencies |
|-------|-------------|--------|--------------|
| 1 | Core Infrastructure | 1-2 days | None |
| 2 | Interceptor Implementation | 1-2 days | Phase 1 |
| 3 | Context Enrichment | 0.5 day | Phase 2 |
| 4 | Sensitive Data Masking | 0.5 day | Phase 2 |
| 5 | Query API | 1 day | Phase 2 |
| 6 | Migration & Testing | 1 day | All phases |

### File Structure (New Files)

```
src/Common/Rtl.Core.Common.Domain/
├── Auditing/
│   ├── IAuditable.cs                    # Marker interface
│   └── SensitiveDataAttribute.cs        # Attribute for explicit marking

src/Common/Rtl.Core.Common.Application/
├── Auditing/
│   ├── IAuditContext.cs                 # Correlation/trace context
│   └── IAuditLogRepository.cs           # Query interface

src/Common/Rtl.Core.Common.Infrastructure/
├── Auditing/
│   ├── AuditLog.cs                      # Entity
│   ├── AuditLogConfiguration.cs         # EF Core config
│   ├── AuditEntry.cs                    # Builder class
│   ├── AuditTrailInterceptor.cs         # Main interceptor
│   ├── AuditContext.cs                  # HTTP context capture
│   ├── AuditMaskingService.cs           # Sensitive field masking
│   └── AuditLogRepository.cs            # Query implementation
```

---

## Phase Details

### Phase 1: Core Infrastructure

**Goal:** Create the foundational types and database schema.

#### Task 1.1: Create Marker Interface

**File:** `src/Common/Rtl.Core.Common.Domain/Auditing/IAuditable.cs`

```csharp
namespace Rtl.Core.Common.Domain.Auditing;

/// <summary>
/// Marker interface for entities that should have field-level audit tracking.
/// Entities implementing this (or inheriting from AuditableEntity) will have
/// their changes logged to the audit_logs table.
/// </summary>
public interface IAuditable;
```

#### Task 1.2: Create Sensitive Data Attribute

**File:** `src/Common/Rtl.Core.Common.Domain/Auditing/SensitiveDataAttribute.cs`

```csharp
namespace Rtl.Core.Common.Domain.Auditing;

/// <summary>
/// Marks a property as containing sensitive data that should be masked in audit logs.
/// Use [NotSensitive] to override convention-based detection.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class SensitiveDataAttribute : Attribute;

/// <summary>
/// Explicitly marks a property as NOT sensitive, overriding convention-based detection.
/// Use when property name contains "password", "secret", etc. but is not actually sensitive.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class NotSensitiveAttribute : Attribute;
```

#### Task 1.3: Create AuditLog Entity

**File:** `src/Common/Rtl.Core.Common.Infrastructure/Auditing/AuditLog.cs`

```csharp
namespace Rtl.Core.Common.Infrastructure.Auditing;

/// <summary>
/// Represents a single audit log entry capturing entity changes.
/// </summary>
public sealed class AuditLog
{
    public Guid Id { get; init; }

    // Entity identification
    public required string EntityName { get; init; }
    public required string EntityId { get; init; }
    public required AuditAction Action { get; init; }

    // Change data (JSON)
    public string? OldValues { get; init; }
    public string? NewValues { get; init; }
    public string? AffectedColumns { get; init; }

    // User context
    public required Guid UserId { get; init; }
    public string? UserName { get; init; }

    // Timing
    public required DateTime TimestampUtc { get; init; }

    // Request context
    public string? CorrelationId { get; init; }
    public string? TraceId { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
}

public enum AuditAction
{
    Unknown = 0,
    Insert = 1,
    Update = 2,
    Delete = 3,
    SoftDelete = 4,
    Restore = 5
}
```

#### Task 1.4: Create EF Core Configuration

**File:** `src/Common/Rtl.Core.Common.Infrastructure/Auditing/AuditLogConfiguration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Rtl.Core.Common.Infrastructure.Auditing;

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.EntityName)
            .HasColumnName("entity_name")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.EntityId)
            .HasColumnName("entity_id")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.Action)
            .HasColumnName("action")
            .IsRequired();

        builder.Property(x => x.OldValues)
            .HasColumnName("old_values")
            .HasColumnType("jsonb");

        builder.Property(x => x.NewValues)
            .HasColumnName("new_values")
            .HasColumnType("jsonb");

        builder.Property(x => x.AffectedColumns)
            .HasColumnName("affected_columns")
            .HasColumnType("jsonb");

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.UserName)
            .HasColumnName("user_name")
            .HasMaxLength(256);

        builder.Property(x => x.TimestampUtc)
            .HasColumnName("timestamp_utc")
            .IsRequired();

        builder.Property(x => x.CorrelationId)
            .HasColumnName("correlation_id")
            .HasMaxLength(256);

        builder.Property(x => x.TraceId)
            .HasColumnName("trace_id")
            .HasMaxLength(256);

        builder.Property(x => x.IpAddress)
            .HasColumnName("ip_address")
            .HasMaxLength(45);

        builder.Property(x => x.UserAgent)
            .HasColumnName("user_agent")
            .HasMaxLength(512);

        // Indexes
        builder.HasIndex(x => new { x.EntityName, x.EntityId })
            .HasDatabaseName("ix_audit_logs_entity");

        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("ix_audit_logs_user");

        builder.HasIndex(x => x.TimestampUtc)
            .HasDatabaseName("ix_audit_logs_timestamp");

        builder.HasIndex(x => x.CorrelationId)
            .HasDatabaseName("ix_audit_logs_correlation");
    }
}
```

#### Task 1.5: Update ModuleDbContext

**File:** `src/Common/Rtl.Core.Common.Infrastructure/Persistence/ModuleDbContext.cs`

Add to `OnModelCreating`:
```csharp
// Audit trail configuration
modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
```

#### Task 1.6: Update IAuditableEntity

**File:** `src/Common/Rtl.Core.Common.Domain/Entities/IAuditableEntity.cs`

Make it extend `IAuditable`:
```csharp
using Rtl.Core.Common.Domain.Auditing;

public interface IAuditableEntity : IAuditable
{
    // existing members...
}
```

---

### Phase 2: Interceptor Implementation

**Goal:** Implement the core audit trail capture logic.

#### Task 2.1: Create AuditEntry Builder

**File:** `src/Common/Rtl.Core.Common.Infrastructure/Auditing/AuditEntry.cs`

```csharp
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace Rtl.Core.Common.Infrastructure.Auditing;

/// <summary>
/// Builder class for constructing AuditLog entries from EF Core change tracking.
/// </summary>
internal sealed class AuditEntry
{
    public EntityEntry Entry { get; }
    public string EntityName { get; }
    public string EntityId { get; private set; } = string.Empty;
    public AuditAction Action { get; set; }
    public Dictionary<string, object?> OldValues { get; } = new();
    public Dictionary<string, object?> NewValues { get; } = new();
    public List<string> AffectedColumns { get; } = new();

    // Context (set externally)
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime TimestampUtc { get; set; }
    public string? CorrelationId { get; set; }
    public string? TraceId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    // For handling temporary keys (e.g., identity columns)
    public List<PropertyEntry> TemporaryProperties { get; } = new();
    public bool HasTemporaryProperties => TemporaryProperties.Count > 0;

    public AuditEntry(EntityEntry entry)
    {
        Entry = entry;
        EntityName = entry.Entity.GetType().Name;
    }

    public void SetEntityId(string entityId)
    {
        EntityId = entityId;
    }

    public AuditLog ToAuditLog()
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            EntityName = EntityName,
            EntityId = EntityId,
            Action = Action,
            OldValues = OldValues.Count > 0
                ? JsonSerializer.Serialize(OldValues)
                : null,
            NewValues = NewValues.Count > 0
                ? JsonSerializer.Serialize(NewValues)
                : null,
            AffectedColumns = AffectedColumns.Count > 0
                ? JsonSerializer.Serialize(AffectedColumns)
                : null,
            UserId = UserId,
            UserName = UserName,
            TimestampUtc = TimestampUtc,
            CorrelationId = CorrelationId,
            TraceId = TraceId,
            IpAddress = IpAddress,
            UserAgent = UserAgent
        };
    }
}
```

#### Task 2.2: Create AuditTrailInterceptor

**File:** `src/Common/Rtl.Core.Common.Infrastructure/Auditing/AuditTrailInterceptor.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Rtl.Core.Common.Application.Identity;
using Rtl.Core.Common.Domain;
using Rtl.Core.Common.Domain.Auditing;

namespace Rtl.Core.Common.Infrastructure.Auditing;

/// <summary>
/// EF Core interceptor that captures field-level changes and writes audit logs.
/// </summary>
public sealed class AuditTrailInterceptor(
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider,
    IAuditContext auditContext,
    IAuditMaskingService maskingService) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
            return base.SavingChangesAsync(eventData, result, cancellationToken);

        var auditEntries = CreateAuditEntries(eventData.Context);

        if (auditEntries.Count == 0)
            return base.SavingChangesAsync(eventData, result, cancellationToken);

        // Store entries without temporary properties
        var immediateEntries = auditEntries
            .Where(e => !e.HasTemporaryProperties)
            .ToList();

        foreach (var entry in immediateEntries)
        {
            eventData.Context.Set<AuditLog>().Add(entry.ToAuditLog());
        }

        // Store entries with temporary properties for post-save processing
        if (auditEntries.Any(e => e.HasTemporaryProperties))
        {
            eventData.Context.ChangeTracker.AutoDetectChangesEnabled = false;

            // Store for SavedChangesAsync to process
            eventData.Context.ChangeTracker.Context
                .GetType()
                .GetProperty("AuditEntriesWithTempKeys")?
                .SetValue(eventData.Context,
                    auditEntries.Where(e => e.HasTemporaryProperties).ToList());
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is null)
            return base.SavingChanges(eventData, result);

        var auditEntries = CreateAuditEntries(eventData.Context);

        foreach (var entry in auditEntries.Where(e => !e.HasTemporaryProperties))
        {
            eventData.Context.Set<AuditLog>().Add(entry.ToAuditLog());
        }

        return base.SavingChanges(eventData, result);
    }

    private List<AuditEntry> CreateAuditEntries(DbContext context)
    {
        var utcNow = dateTimeProvider.UtcNow;
        var userId = currentUserService.UserId ?? Guid.Empty;
        var auditEntries = new List<AuditEntry>();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            // Skip non-auditable entities
            if (entry.Entity is not IAuditable)
                continue;

            // Skip AuditLog itself to prevent recursion
            if (entry.Entity is AuditLog)
                continue;

            // Skip unchanged or detached
            if (entry.State is EntityState.Unchanged or EntityState.Detached)
                continue;

            var auditEntry = new AuditEntry(entry)
            {
                UserId = userId,
                UserName = auditContext.UserName,
                TimestampUtc = utcNow,
                CorrelationId = auditContext.CorrelationId,
                TraceId = auditContext.TraceId,
                IpAddress = auditContext.IpAddress,
                UserAgent = auditContext.UserAgent,
                Action = DetermineAction(entry)
            };

            foreach (var property in entry.Properties)
            {
                // Skip shadow properties (except foreign keys we want to track)
                if (property.Metadata.IsShadowProperty() &&
                    !property.Metadata.IsForeignKey())
                    continue;

                var propertyName = property.Metadata.Name;

                // Handle primary keys
                if (property.Metadata.IsPrimaryKey())
                {
                    if (property.IsTemporary)
                    {
                        // Key will be generated by database
                        auditEntry.TemporaryProperties.Add(property);
                    }
                    else
                    {
                        auditEntry.SetEntityId(property.CurrentValue?.ToString() ?? "");
                    }
                    continue;
                }

                // Mask sensitive values
                var oldValue = maskingService.MaskIfSensitive(
                    propertyName,
                    property.OriginalValue,
                    entry.Entity.GetType());

                var newValue = maskingService.MaskIfSensitive(
                    propertyName,
                    property.CurrentValue,
                    entry.Entity.GetType());

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.NewValues[propertyName] = newValue;
                        break;

                    case EntityState.Deleted:
                        auditEntry.OldValues[propertyName] = oldValue;
                        break;

                    case EntityState.Modified:
                        if (property.IsModified &&
                            !Equals(property.OriginalValue, property.CurrentValue))
                        {
                            auditEntry.OldValues[propertyName] = oldValue;
                            auditEntry.NewValues[propertyName] = newValue;
                            auditEntry.AffectedColumns.Add(propertyName);
                        }
                        break;
                }
            }

            // Only add if there are actual changes to record
            if (auditEntry.OldValues.Count > 0 ||
                auditEntry.NewValues.Count > 0 ||
                entry.State == EntityState.Deleted)
            {
                auditEntries.Add(auditEntry);
            }
        }

        return auditEntries;
    }

    private static AuditAction DetermineAction(EntityEntry entry)
    {
        if (entry.State == EntityState.Added)
            return AuditAction.Insert;

        if (entry.State == EntityState.Deleted)
            return AuditAction.Delete;

        if (entry.State == EntityState.Modified)
        {
            // Check for soft delete transition
            var isDeletedProp = entry.Properties
                .FirstOrDefault(p => p.Metadata.Name.Equals(
                    "IsDeleted", StringComparison.OrdinalIgnoreCase));

            if (isDeletedProp is not null)
            {
                var wasDeleted = isDeletedProp.OriginalValue as bool? ?? false;
                var isDeleted = isDeletedProp.CurrentValue as bool? ?? false;

                if (!wasDeleted && isDeleted)
                    return AuditAction.SoftDelete;

                if (wasDeleted && !isDeleted)
                    return AuditAction.Restore;
            }

            return AuditAction.Update;
        }

        return AuditAction.Unknown;
    }
}
```

#### Task 2.3: Register Interceptor

**File:** `src/Common/Rtl.Core.Common.Infrastructure/Persistence/ModuleDbContextExtensions.cs`

Add `AuditTrailInterceptor` to the interceptor chain:
```csharp
.AddInterceptors(
    sp.GetRequiredService<CacheWriteGuardInterceptor>(),
    sp.GetRequiredService<SoftDeleteInterceptor>(),
    sp.GetRequiredService<AuditableEntitiesInterceptor>(),
    sp.GetRequiredService<AuditTrailInterceptor>(),  // NEW - after AuditableEntitiesInterceptor
    sp.GetRequiredService<InsertOutboxMessagesInterceptor>());
```

**File:** `src/Common/Rtl.Core.Common.Infrastructure/InfrastructureConfiguration.cs`

Add to `AddAuditingServices`:
```csharp
services.TryAddScoped<AuditTrailInterceptor>();
services.TryAddScoped<IAuditContext, AuditContext>();
services.TryAddSingleton<IAuditMaskingService, AuditMaskingService>();
```

---

### Phase 3: Context Enrichment

**Goal:** Capture HTTP request context for correlation and debugging.

#### Task 3.1: Create IAuditContext Interface

**File:** `src/Common/Rtl.Core.Common.Application/Auditing/IAuditContext.cs`

```csharp
namespace Rtl.Core.Common.Application.Auditing;

/// <summary>
/// Provides contextual information for audit log entries.
/// </summary>
public interface IAuditContext
{
    string? UserName { get; }
    string? CorrelationId { get; }
    string? TraceId { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
}
```

#### Task 3.2: Create AuditContext Implementation

**File:** `src/Common/Rtl.Core.Common.Infrastructure/Auditing/AuditContext.cs`

```csharp
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Rtl.Core.Common.Application.Auditing;

namespace Rtl.Core.Common.Infrastructure.Auditing;

/// <summary>
/// Captures audit context from HTTP request and distributed tracing.
/// </summary>
internal sealed class AuditContext : IAuditContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserName =>
        _httpContextAccessor.HttpContext?.User?.Identity?.Name;

    public string? CorrelationId =>
        _httpContextAccessor.HttpContext?.TraceIdentifier;

    public string? TraceId =>
        Activity.Current?.TraceId.ToString();

    public string? IpAddress =>
        GetClientIpAddress();

    public string? UserAgent =>
        _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();

    private string? GetClientIpAddress()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null) return null;

        // Check for forwarded header (behind proxy/load balancer)
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        return httpContext.Connection.RemoteIpAddress?.ToString();
    }
}
```

---

### Phase 4: Sensitive Data Masking

**Goal:** Prevent sensitive data from appearing in audit logs.

#### Task 4.1: Create IAuditMaskingService Interface

**File:** `src/Common/Rtl.Core.Common.Application/Auditing/IAuditMaskingService.cs`

```csharp
namespace Rtl.Core.Common.Application.Auditing;

/// <summary>
/// Service for masking sensitive data in audit logs.
/// </summary>
public interface IAuditMaskingService
{
    /// <summary>
    /// Returns masked value if property is sensitive, otherwise returns original value.
    /// </summary>
    object? MaskIfSensitive(string propertyName, object? value, Type entityType);
}
```

#### Task 4.2: Create AuditMaskingService Implementation

**File:** `src/Common/Rtl.Core.Common.Infrastructure/Auditing/AuditMaskingService.cs`

```csharp
using System.Collections.Concurrent;
using System.Reflection;
using Rtl.Core.Common.Application.Auditing;
using Rtl.Core.Common.Domain.Auditing;

namespace Rtl.Core.Common.Infrastructure.Auditing;

/// <summary>
/// Masks sensitive property values using convention and attribute-based detection.
/// </summary>
internal sealed class AuditMaskingService : IAuditMaskingService
{
    private const string MaskedValue = "***REDACTED***";

    private static readonly string[] SensitiveKeywords =
    {
        "password", "secret", "token", "apikey", "api_key",
        "connectionstring", "connection_string", "credential",
        "privatekey", "private_key", "ssn", "creditcard", "credit_card"
    };

    // Cache sensitivity checks per property
    private readonly ConcurrentDictionary<(Type, string), bool> _sensitivityCache = new();

    public object? MaskIfSensitive(string propertyName, object? value, Type entityType)
    {
        if (value is null)
            return null;

        var isSensitive = _sensitivityCache.GetOrAdd(
            (entityType, propertyName),
            key => CheckSensitivity(key.Item1, key.Item2));

        return isSensitive ? MaskedValue : value;
    }

    private static bool CheckSensitivity(Type entityType, string propertyName)
    {
        var property = entityType.GetProperty(propertyName);

        // Check for explicit [NotSensitive] attribute
        if (property?.GetCustomAttribute<NotSensitiveAttribute>() is not null)
            return false;

        // Check for explicit [SensitiveData] attribute
        if (property?.GetCustomAttribute<SensitiveDataAttribute>() is not null)
            return true;

        // Convention-based: check property name
        var lowerName = propertyName.ToLowerInvariant();
        return SensitiveKeywords.Any(keyword => lowerName.Contains(keyword));
    }
}
```

---

### Phase 5: Query API

**Goal:** Provide interfaces for querying audit history.

#### Task 5.1: Create IAuditLogRepository Interface

**File:** `src/Common/Rtl.Core.Common.Application/Auditing/IAuditLogRepository.cs`

```csharp
namespace Rtl.Core.Common.Application.Auditing;

/// <summary>
/// Repository for querying audit logs.
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Gets audit history for a specific entity.
    /// </summary>
    Task<IReadOnlyList<AuditLogDto>> GetEntityHistoryAsync(
        string entityName,
        string entityId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs by correlation ID (same HTTP request).
    /// </summary>
    Task<IReadOnlyList<AuditLogDto>> GetByCorrelationIdAsync(
        string correlationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs for a specific user within a date range.
    /// </summary>
    Task<IReadOnlyList<AuditLogDto>> GetByUserAsync(
        Guid userId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default);
}

public sealed record AuditLogDto(
    Guid Id,
    string EntityName,
    string EntityId,
    string Action,
    string? OldValues,
    string? NewValues,
    string? AffectedColumns,
    Guid UserId,
    string? UserName,
    DateTime TimestampUtc,
    string? CorrelationId);
```

#### Task 5.2: Create AuditLogRepository Implementation

**File:** `src/Common/Rtl.Core.Common.Infrastructure/Auditing/AuditLogRepository.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Rtl.Core.Common.Application.Auditing;

namespace Rtl.Core.Common.Infrastructure.Auditing;

internal sealed class AuditLogRepository<TContext> : IAuditLogRepository
    where TContext : DbContext
{
    private readonly TContext _context;

    public AuditLogRepository(TContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetEntityHistoryAsync(
        string entityName,
        string entityId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<AuditLog>()
            .Where(a => a.EntityName == entityName && a.EntityId == entityId)
            .OrderByDescending(a => a.TimestampUtc)
            .Select(a => ToDto(a))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetByCorrelationIdAsync(
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<AuditLog>()
            .Where(a => a.CorrelationId == correlationId)
            .OrderBy(a => a.TimestampUtc)
            .Select(a => ToDto(a))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetByUserAsync(
        Guid userId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<AuditLog>()
            .Where(a => a.UserId == userId &&
                        a.TimestampUtc >= fromUtc &&
                        a.TimestampUtc <= toUtc)
            .OrderByDescending(a => a.TimestampUtc)
            .Select(a => ToDto(a))
            .ToListAsync(cancellationToken);
    }

    private static AuditLogDto ToDto(AuditLog a) => new(
        a.Id,
        a.EntityName,
        a.EntityId,
        a.Action.ToString(),
        a.OldValues,
        a.NewValues,
        a.AffectedColumns,
        a.UserId,
        a.UserName,
        a.TimestampUtc,
        a.CorrelationId);
}
```

---

### Phase 6: Migration & Testing

#### Task 6.1: Create Migrations

Run for each module:
```bash
# Sales module
dotnet ef migrations add AddAuditLogs -p src/Modules/Sales/Infrastructure -s src/Api/Host

# Orders module
dotnet ef migrations add AddAuditLogs -p src/Modules/SampleOrders/Infrastructure -s src/Api/Host
```

#### Task 6.2: Create Unit Tests

**File:** `test/Rtl.Core.Common.Infrastructure.Tests/Auditing/AuditTrailInterceptorTests.cs`

Test cases:
- Insert creates audit log with NewValues only
- Update creates audit log with OldValues, NewValues, and AffectedColumns
- Delete creates audit log with OldValues only
- Soft delete detected correctly (Action = SoftDelete)
- Restore detected correctly (Action = Restore)
- Sensitive fields are masked
- Non-IAuditable entities are skipped
- AuditLog entity itself is skipped (no recursion)

#### Task 6.3: Create Integration Tests

Verify:
- Audit logs persisted in same transaction
- Rollback also rolls back audit logs
- Correlation ID captured from HTTP context
- User ID captured correctly

---

## Code Examples

### Example: Viewing Product Change History

```csharp
// In a query handler or controller
public async Task<IReadOnlyList<AuditLogDto>> GetProductHistory(
    Guid productId,
    [FromServices] IAuditLogRepository auditRepository)
{
    return await auditRepository.GetEntityHistoryAsync(
        entityName: "Product",
        entityId: productId.ToString());
}
```

### Example: Audit Log Entry (JSON)

```json
{
  "id": "a1b2c3d4-...",
  "entityName": "Product",
  "entityId": "f5e6d7c8-...",
  "action": "Update",
  "oldValues": "{\"Price\": 9.99, \"Name\": \"Widget\"}",
  "newValues": "{\"Price\": 12.99, \"Name\": \"Super Widget\"}",
  "affectedColumns": "[\"Price\", \"Name\"]",
  "userId": "user-guid-...",
  "userName": "john.doe@example.com",
  "timestampUtc": "2026-01-26T12:34:56Z",
  "correlationId": "req-12345",
  "traceId": "abc123def456",
  "ipAddress": "192.168.1.100",
  "userAgent": "Mozilla/5.0..."
}
```

---

## Migration Strategy

### Approach: Additive (Non-Breaking)

1. **Keep existing audit fields** on entities (CreatedAt, ModifiedAt, etc.)
2. **Add new audit_logs table** via migration
3. **Both systems run in parallel** - embedded fields for quick access, audit_logs for history
4. **No data migration needed** - only new changes are tracked

### Rollback Plan

If issues arise:
1. Remove `AuditTrailInterceptor` from interceptor chain
2. Keep audit_logs table (historical data preserved)
3. System continues with existing embedded audit fields

---

## Testing Strategy

### Unit Tests
- `AuditTrailInterceptorTests` - Core interceptor logic
- `AuditMaskingServiceTests` - Sensitive field detection
- `AuditEntryTests` - Builder class

### Integration Tests
- Full SaveChanges flow with audit capture
- Transaction rollback verification
- HTTP context capture

### Manual Testing Checklist
- [ ] Create entity → audit log with Insert action
- [ ] Update entity → audit log with Update action and field changes
- [ ] Soft delete entity → audit log with SoftDelete action
- [ ] Restore entity → audit log with Restore action
- [ ] Update with sensitive field → value is masked
- [ ] Query entity history returns ordered results

---

## Dependencies

### NuGet Packages (Already Present)
- `Microsoft.EntityFrameworkCore` - Change tracking
- `System.Text.Json` - JSON serialization
- `Microsoft.AspNetCore.Http` - HTTP context access

### No New Packages Required

---

## Files to Create (Summary)

| Phase | File | Type |
|-------|------|------|
| 1 | `Common.Domain/Auditing/IAuditable.cs` | Interface |
| 1 | `Common.Domain/Auditing/SensitiveDataAttribute.cs` | Attribute |
| 1 | `Common.Infrastructure/Auditing/AuditLog.cs` | Entity |
| 1 | `Common.Infrastructure/Auditing/AuditLogConfiguration.cs` | EF Config |
| 2 | `Common.Infrastructure/Auditing/AuditEntry.cs` | Builder |
| 2 | `Common.Infrastructure/Auditing/AuditTrailInterceptor.cs` | Interceptor |
| 3 | `Common.Application/Auditing/IAuditContext.cs` | Interface |
| 3 | `Common.Infrastructure/Auditing/AuditContext.cs` | Implementation |
| 4 | `Common.Application/Auditing/IAuditMaskingService.cs` | Interface |
| 4 | `Common.Infrastructure/Auditing/AuditMaskingService.cs` | Implementation |
| 5 | `Common.Application/Auditing/IAuditLogRepository.cs` | Interface |
| 5 | `Common.Infrastructure/Auditing/AuditLogRepository.cs` | Implementation |

## Files to Modify (Summary)

| File | Change |
|------|--------|
| `Common.Domain/Entities/IAuditableEntity.cs` | Extend IAuditable |
| `Common.Infrastructure/Persistence/ModuleDbContext.cs` | Add AuditLogConfiguration |
| `Common.Infrastructure/Persistence/ModuleDbContextExtensions.cs` | Add AuditTrailInterceptor |
| `Common.Infrastructure/InfrastructureConfiguration.cs` | Register services |

---

## Success Criteria

1. All entity changes logged to `audit_logs` table
2. Field-level changes captured (old/new values)
3. Soft delete/restore detected correctly
4. Sensitive fields masked
5. HTTP context captured (correlation ID, IP, user agent)
6. Query API functional
7. All existing tests pass
8. No performance regression on SaveChanges

---

## References

- **dotnet-starter-kit Auditing Module:** `Z:\dotnet-starter-kit\src\Modules\Auditing\`
- **Research Document:** This file
- **Current Interceptors:** `X:\SES\src\Common\Rtl.Core.Common.Infrastructure\Auditing\`
