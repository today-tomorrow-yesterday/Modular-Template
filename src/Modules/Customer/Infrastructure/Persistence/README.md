# Customer Module - Migrations

All commands should be run from the repository root (where `Rtl.Core.Api.sln` lives).

## Add a New Migration

```bash
dotnet ef migrations add <MigrationName> \
  --project src/Modules/Customer/Infrastructure \
  --startup-project src/Api/Host \
  --context CustomerDbContext \
  --output-dir Persistence/Migrations
```

## Update Database

```bash
dotnet ef database update \
  --project src/Modules/Customer/Infrastructure \
  --startup-project src/Api/Host \
  --context CustomerDbContext
```

## Remove Last Migration (if not applied)

```bash
dotnet ef migrations remove \
  --project src/Modules/Customer/Infrastructure \
  --startup-project src/Api/Host \
  --context CustomerDbContext
```

## List Migrations

```bash
dotnet ef migrations list \
  --project src/Modules/Customer/Infrastructure \
  --startup-project src/Api/Host \
  --context CustomerDbContext
```

## Generate SQL Script

```bash
dotnet ef migrations script \
  --project src/Modules/Customer/Infrastructure \
  --startup-project src/Api/Host \
  --context CustomerDbContext \
  --output customer_migrations.sql
```

## Details

| Property | Value |
|----------|-------|
| DbContext | `CustomerDbContext` |
| Schema | `customers` |
| Sequence | `customers_hilo_seq` |
