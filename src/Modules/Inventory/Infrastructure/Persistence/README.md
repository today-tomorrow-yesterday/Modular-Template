# Inventory Module - Migrations

All commands should be run from the repository root (where `Rtl.Core.Api.sln` lives).

## Add a New Migration

```bash
dotnet ef migrations add <MigrationName> \
  --project src/Modules/Inventory/Infrastructure \
  --startup-project src/Api/Host \
  --context InventoryDbContext \
  --output-dir Persistence/Migrations
```

## Update Database

```bash
dotnet ef database update \
  --project src/Modules/Inventory/Infrastructure \
  --startup-project src/Api/Host \
  --context InventoryDbContext
```

## Remove Last Migration (if not applied)

```bash
dotnet ef migrations remove \
  --project src/Modules/Inventory/Infrastructure \
  --startup-project src/Api/Host \
  --context InventoryDbContext
```

## List Migrations

```bash
dotnet ef migrations list \
  --project src/Modules/Inventory/Infrastructure \
  --startup-project src/Api/Host \
  --context InventoryDbContext
```

## Generate SQL Script

```bash
dotnet ef migrations script \
  --project src/Modules/Inventory/Infrastructure \
  --startup-project src/Api/Host \
  --context InventoryDbContext \
  --output inventory_migrations.sql
```

## Details

| Property | Value |
|----------|-------|
| DbContext | `InventoryDbContext` |
| Schema | `inventories` |
| Sequence | `inventories_hilo_seq` |
