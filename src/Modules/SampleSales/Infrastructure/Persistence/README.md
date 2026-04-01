# SampleSales Module - Migrations

All commands should be run from the repository root (where `ModularTemplate.Api.sln` lives).

> **Note:** This module uses `[SensitiveData]` attributes with `EncryptionValueConverter`.
> You must set the `ENCRYPTION_KEY` environment variable before running migration commands.
>
> ```bash
> # PowerShell
> $env:ENCRYPTION_KEY = "<your-base64-encryption-key>"
>
> # Bash
> export ENCRYPTION_KEY="<your-base64-encryption-key>"
> ```

## Add a New Migration

```bash
dotnet ef migrations add <MigrationName> \
  --project src/Modules/SampleSales/Infrastructure \
  --startup-project src/Api/Host \
  --context SampleDbContext \
  --output-dir Persistence/Migrations
```

## Update Database

```bash
dotnet ef database update \
  --project src/Modules/SampleSales/Infrastructure \
  --startup-project src/Api/Host \
  --context SampleDbContext
```

## Remove Last Migration (if not applied)

```bash
dotnet ef migrations remove \
  --project src/Modules/SampleSales/Infrastructure \
  --startup-project src/Api/Host \
  --context SampleDbContext
```

## List Migrations

```bash
dotnet ef migrations list \
  --project src/Modules/SampleSales/Infrastructure \
  --startup-project src/Api/Host \
  --context SampleDbContext
```

## Generate SQL Script

```bash
dotnet ef migrations script \
  --project src/Modules/SampleSales/Infrastructure \
  --startup-project src/Api/Host \
  --context SampleDbContext \
  --output sample_sales_migrations.sql
```

## Details

| Property | Value |
|----------|-------|
| DbContext | `SampleDbContext` |
| Schema | `sample` |
| Sequence | `sample_hilo_seq` |
| Requires `ENCRYPTION_KEY` | Yes |
