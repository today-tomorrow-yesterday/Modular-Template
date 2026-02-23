# SampleOrders Module - Migrations

All commands should be run from the repository root (where `Rtl.Core.Api.sln` lives).

## Add a New Migration

```bash
dotnet ef migrations add <MigrationName> \
  --project src/Modules/SampleOrders/Infrastructure \
  --startup-project src/Api/Host \
  --context OrdersDbContext \
  --output-dir Persistence/Migrations
```

## Update Database

```bash
dotnet ef database update \
  --project src/Modules/SampleOrders/Infrastructure \
  --startup-project src/Api/Host \
  --context OrdersDbContext
```

## Remove Last Migration (if not applied)

```bash
dotnet ef migrations remove \
  --project src/Modules/SampleOrders/Infrastructure \
  --startup-project src/Api/Host \
  --context OrdersDbContext
```

## List Migrations

```bash
dotnet ef migrations list \
  --project src/Modules/SampleOrders/Infrastructure \
  --startup-project src/Api/Host \
  --context OrdersDbContext
```

## Generate SQL Script

```bash
dotnet ef migrations script \
  --project src/Modules/SampleOrders/Infrastructure \
  --startup-project src/Api/Host \
  --context OrdersDbContext \
  --output sample_orders_migrations.sql
```

## Details

| Property | Value |
|----------|-------|
| DbContext | `OrdersDbContext` |
| Schema | `orders` |
| Sequence | `orders_hilo_seq` |
