# Funding Module - Migrations

All commands should be run from the repository root (where `Rtl.Core.Api.sln` lives).

## Add a New Migration

```bash
dotnet ef migrations add <MigrationName> \
  --project src/Modules/Funding/Infrastructure \
  --startup-project src/Api/Host \
  --context FundingDbContext \
  --output-dir Persistence/Migrations
```

## Update Database

```bash
dotnet ef database update \
  --project src/Modules/Funding/Infrastructure \
  --startup-project src/Api/Host \
  --context FundingDbContext
```

## Remove Last Migration (if not applied)

```bash
dotnet ef migrations remove \
  --project src/Modules/Funding/Infrastructure \
  --startup-project src/Api/Host \
  --context FundingDbContext
```

## List Migrations

```bash
dotnet ef migrations list \
  --project src/Modules/Funding/Infrastructure \
  --startup-project src/Api/Host \
  --context FundingDbContext
```

## Generate SQL Script

```bash
dotnet ef migrations script \
  --project src/Modules/Funding/Infrastructure \
  --startup-project src/Api/Host \
  --context FundingDbContext \
  --output funding_migrations.sql
```

## Details

| Property | Value |
|----------|-------|
| DbContext | `FundingDbContext` |
| Schema | `fundings` |
| Sequence | `fundings_hilo_seq` |
