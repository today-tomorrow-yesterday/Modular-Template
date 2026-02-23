# Sales Module - Migrations

All commands should be run from the repository root (where `Rtl.Core.Api.sln` lives).

## Add a New Migration

```bash
dotnet ef migrations add <MigrationName> \
  --project src/Modules/Sales/Infrastructure \
  --startup-project src/Api/Host \
  --context SalesDbContext \
  --output-dir Persistence/Migrations
```

## Update Database

```bash
dotnet ef database update \
  --project src/Modules/Sales/Infrastructure \
  --startup-project src/Api/Host \
  --context SalesDbContext
```

## Remove Last Migration (if not applied)

```bash
dotnet ef migrations remove \
  --project src/Modules/Sales/Infrastructure \
  --startup-project src/Api/Host \
  --context SalesDbContext
```

## List Migrations

```bash
dotnet ef migrations list \
  --project src/Modules/Sales/Infrastructure \
  --startup-project src/Api/Host \
  --context SalesDbContext
```

## Generate SQL Script

```bash
dotnet ef migrations script \
  --project src/Modules/Sales/Infrastructure \
  --startup-project src/Api/Host \
  --context SalesDbContext \
  --output sales_migrations.sql
```

## Details

| Property | Value |
|----------|-------|
| DbContext | `SalesDbContext` |
| Schema | `sales` |
| Sequence | `sales_hilo_seq` |
