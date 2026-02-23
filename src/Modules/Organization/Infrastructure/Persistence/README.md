# Organization Module - Migrations

All commands should be run from the repository root (where `Rtl.Core.Api.sln` lives).

## Add a New Migration

```bash
dotnet ef migrations add <MigrationName> \
  --project src/Modules/Organization/Infrastructure \
  --startup-project src/Api/Host \
  --context OrganizationDbContext \
  --output-dir Persistence/Migrations
```

## Update Database

```bash
dotnet ef database update \
  --project src/Modules/Organization/Infrastructure \
  --startup-project src/Api/Host \
  --context OrganizationDbContext
```

## Remove Last Migration (if not applied)

```bash
dotnet ef migrations remove \
  --project src/Modules/Organization/Infrastructure \
  --startup-project src/Api/Host \
  --context OrganizationDbContext
```

## List Migrations

```bash
dotnet ef migrations list \
  --project src/Modules/Organization/Infrastructure \
  --startup-project src/Api/Host \
  --context OrganizationDbContext
```

## Generate SQL Script

```bash
dotnet ef migrations script \
  --project src/Modules/Organization/Infrastructure \
  --startup-project src/Api/Host \
  --context OrganizationDbContext \
  --output organization_migrations.sql
```

## Details

| Property | Value |
|----------|-------|
| DbContext | `OrganizationDbContext` |
| Schema | `organizations` |
| Sequence | `organizations_hilo_seq` |
