---
name: migration-workflow
description: Use when generating, applying, or troubleshooting EF Core migrations - covers the ENCRYPTION_KEY requirement, multi-module migration commands, and safe migration practices
---

# Migration Workflow

## Prerequisites

**ENCRYPTION_KEY environment variable is REQUIRED.** The project uses AES-256-GCM encryption for `[SensitiveData]` fields. Without this key, the DbContext cannot build the model and migrations will fail.

```bash
# Set before running any EF command
export ENCRYPTION_KEY="MTIzNDU2Nzg5MDEyMzQ1Njc4OTAxMjM0NTY3ODkwMTI="
```

The value is a base64-encoded 32-byte key. The same test key is in `launchSettings.json` for development.

## Generate Migration

```bash
ENCRYPTION_KEY="MTIzNDU2Nzg5MDEyMzQ1Njc4OTAxMjM0NTY3ODkwMTI=" \
dotnet ef migrations add {MigrationName} \
  --project src/Modules/{Module}/Infrastructure \
  --startup-project src/Api/Host \
  --context {Module}DbContext \
  --output-dir Persistence/Migrations
```

### Module-Specific Commands

**SampleOrders:**
```bash
--context OrdersDbContext
```

**SampleSales:**
```bash
--context SampleDbContext
```

## Apply Migration

```bash
ENCRYPTION_KEY="..." dotnet ef database update \
  --project src/Modules/{Module}/Infrastructure \
  --startup-project src/Api/Host \
  --context {Module}DbContext
```

## Remove Last Migration

```bash
ENCRYPTION_KEY="..." dotnet ef migrations remove \
  --project src/Modules/{Module}/Infrastructure \
  --startup-project src/Api/Host \
  --context {Module}DbContext
```

## Migration Naming Conventions

| Change | Name Pattern | Example |
|--------|-------------|---------|
| New table/entity | `Add{Entity}` | `AddRepoHome` |
| New column | `Add{Column}To{Table}` | `AddPublicIdToOrders` |
| Drop table | `Drop{Entity}` | `DropProductCache` |
| Column rename | `Rename{Old}To{New}` | `RenameStatusToOrderStatus` |
| Type change | `Replace{Old}With{New}` | `ReplaceIntRefWithGuidPublicId` |
| Combined changes | Descriptive summary | `AddOrderLineTPHAndShippingAddress` |

## Data Loss Warnings

EF will flag `An operation was scaffolded that may result in the loss of data` when:
- Dropping columns
- Changing column types (e.g., `int` → `uuid`)
- Dropping tables

**Always review the migration `.cs` file** after generation. Check that:
- Drop operations are intentional
- Column type changes are expected
- Default values make sense for new NOT NULL columns

## Multi-Module Changes

When a refactoring spans multiple modules (e.g., changing an integration event contract), you may need migrations in multiple modules:

```bash
# 1. Generate for the producer module
ENCRYPTION_KEY="..." dotnet ef migrations add {Name} \
  --project src/Modules/SampleOrders/Infrastructure \
  --startup-project src/Api/Host \
  --context OrdersDbContext --output-dir Persistence/Migrations

# 2. Generate for the consumer module
ENCRYPTION_KEY="..." dotnet ef migrations add {Name} \
  --project src/Modules/SampleSales/Infrastructure \
  --startup-project src/Api/Host \
  --context SampleDbContext --output-dir Persistence/Migrations
```

## Troubleshooting

**"Unable to resolve service for type 'EncryptionService'"**
→ `ENCRYPTION_KEY` environment variable not set.

**"The model has changed since the last migration"**
→ You have code changes that haven't been captured in a migration. Run `migrations add`.

**Migration generates unexpected changes**
→ Someone may have modified model code without generating a migration. Check `git diff` on the DbContext and entity configurations.

## Checklist

- [ ] `ENCRYPTION_KEY` env var set before running EF commands
- [ ] Migration name is descriptive PascalCase
- [ ] Reviewed generated `.cs` file for data loss operations
- [ ] Default values make sense for new NOT NULL columns
- [ ] Build succeeds after migration generation
- [ ] If multi-module change, migrations generated for ALL affected modules
