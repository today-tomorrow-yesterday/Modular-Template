---
name: ef-core-patterns
description: Use when writing EF Core configurations, JSONB details classes, versioned converters, encryption, or generating migrations - covers all persistence patterns in this project
---

# EF Core Patterns

## JSONB Details with Versioning

JSONB columns use `VersionedJsonConverter<T>` (NOT `OwnsOne`/`ToJson`) for forward-compatible schema evolution.

### Details Class

```csharp
using Rtl.Core.Domain.Entities;

namespace Modules.{Module}.Domain.{Feature};

public sealed class {Entity}Details : IVersionedDetails
{
    public int SchemaVersion { get; set; } = 1;

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }

    // Domain-specific properties
    public string? SomeField { get; set; }
    public decimal? SomeAmount { get; set; }
}
```

**Rules:**
- Always implement `IVersionedDetails` (from `Rtl.Core.Domain.Entities`)
- Always include `SchemaVersion` (default = current version)
- Always include `[JsonExtensionData] ExtensionData` (captures unknown fields during deserialization)
- Use `System.Text.Json` attributes (NOT Newtonsoft)

### EF Configuration for JSONB

```csharp
builder.Property(e => e.Details)
    .HasColumnName("details")
    .HasColumnType("jsonb")
    .HasConversion(new VersionedJsonConverter<{Entity}Details>());
```

### Version Upgrades

When the schema evolves, create an upgrader:

```csharp
public sealed class {Entity}DetailsV2Upgrader : IDetailsUpgrader<{Entity}Details>
{
    public int FromVersion => 1;
    public int ToVersion => 2;

    public {Entity}Details Upgrade({Entity}Details details)
    {
        // Transform v1 → v2
        details.SchemaVersion = 2;
        return details;
    }
}
```

Register in `DetailsVersionRegistry<T>`:
```csharp
var registry = new DetailsVersionRegistry<{Entity}Details>();
registry.Register(new {Entity}DetailsV2Upgrader());
// Pass to converter: new VersionedJsonConverter<{Entity}Details>(registry)
```

## Sensitive Data Encryption

Fields marked with `[SensitiveData]` are automatically encrypted at rest using AES-256-GCM via EF value converters.

```csharp
// Domain entity
[SensitiveData] public string? SerialNumber { get; private set; }
[SensitiveData] public decimal? TotalInvoiceAmount { get; private set; }

// For JSONB fields, use JsonEncryptionValueConverter in the EF config
```

**Requires:** `ENCRYPTION_KEY` environment variable (base64-encoded 32-byte key) at runtime and during migrations.

## EF Configuration Conventions

### Column Naming
All columns use `snake_case`:
```csharp
builder.Property(e => e.RefHomeCenterNumber).HasColumnName("ref_home_center_number");
builder.Property(e => e.LastSyncedAtUtc).HasColumnName("last_synced_at_utc");
```

### Primary Keys

**Source-of-truth entities** (module owns the data):
```csharp
builder.Property(e => e.Id).HasColumnName("id").UseHiLo($"seq_{table}");
```

**CDC cache entities** (iSeries provides the ID):
```csharp
builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
```

**Consumer cache entities** (Sales owns the local cache key):
```csharp
builder.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
```

### Index Naming
```csharp
builder.HasIndex(e => e.PublicId).IsUnique()
    .HasDatabaseName("ix_{table}_public_id");

builder.HasIndex(e => new { e.RefHomeCenterNumber, e.RefStockNumber }).IsUnique()
    .HasDatabaseName("ix_{table}_hc_stock");
```

### Enum Conversion
```csharp
builder.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
```

### Ignore DomainEvents
```csharp
builder.Ignore(e => e.DomainEvents);  // Required for entities extending Entity
```

## Migration Workflow

### Generate Migration
```bash
# Set encryption key first
export ENCRYPTION_KEY="MTIzNDU2Nzg5MDEyMzQ1Njc4OTAxMjM0NTY3ODkwMTI="

# From repo root
dotnet ef migrations add {MigrationName} \
  --project src/Modules/{Module}/Infrastructure \
  --startup-project src/Api/Host \
  --context {Module}DbContext \
  --output-dir Persistence/Migrations
```

### Other Commands
```bash
# Apply migrations
dotnet ef database update --project src/Modules/{Module}/Infrastructure \
  --startup-project src/Api/Host --context {Module}DbContext

# Remove last migration
dotnet ef migrations remove --project src/Modules/{Module}/Infrastructure \
  --startup-project src/Api/Host --context {Module}DbContext

# Generate SQL script
dotnet ef migrations script --project src/Modules/{Module}/Infrastructure \
  --startup-project src/Api/Host --context {Module}DbContext
```

### Migration Naming
- `InitialCreate` — first migration
- `Add{Feature}` — new tables/columns
- `Drop{Feature}` — removing tables
- `Rename{Old}To{New}` — column/table renames
- Descriptive names, PascalCase, no dates in the name (EF adds timestamps)

## TPH (Table Per Hierarchy) Pattern

Used for package lines in Sales:
```csharp
builder.HasDiscriminator<string>("line_type")
    .HasValue<HomeLine>("Home")
    .HasValue<LandLine>("Land")
    .HasValue<ProjectCostLine>("ProjectCost");
```

## Checklist

- [ ] All columns use snake_case naming
- [ ] JSONB fields use `VersionedJsonConverter<T>`, not `OwnsOne`/`ToJson`
- [ ] Details classes implement `IVersionedDetails` with `SchemaVersion` + `ExtensionData`
- [ ] Sensitive fields marked with `[SensitiveData]`
- [ ] DomainEvents ignored on Entity-derived classes
- [ ] Correct PK strategy (HiLo vs ValueGeneratedNever vs UseIdentityAlwaysColumn)
- [ ] Index names follow `ix_{table}_{column}` pattern
- [ ] Migration generated with `ENCRYPTION_KEY` env var set
