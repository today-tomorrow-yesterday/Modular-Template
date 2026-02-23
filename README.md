# Rtl.Core.Api - SES Pro Modular Monolith

A production-ready .NET 10 modular monolith with DDD, Clean Architecture, and CQRS.

## Features

- **Modular Architecture** - Loosely coupled modules with clear boundaries
- **DDD & CQRS** - Domain events, aggregate roots, separate command/query paths
- **Result Pattern** - Railway-oriented error handling
- **Outbox/Inbox** - Reliable event publishing
- **Feature Flags** - Two-tier toggles (infrastructure + module-level)
- **API Versioning** - URL segment versioning (v1/v2)

## Feature Flags

Two-tier system designed for microservice extraction:

| Scope | Location | Example Key |
|-------|----------|-------------|
| Infrastructure | `appsettings.json` | `Features:Infrastructure:Outbox` |
| Module | `modules.{module}.json` | `Sales:Features:CatalogV2Pagination` |

**Infrastructure flags**: `Outbox`, `Inbox`, `BackgroundJobs`, `Emails`, `CdcEvents` - when disabled, messages remain queued.

**Usage**: `.RequireFeature()` on endpoints (returns 404 when disabled) or `IFeatureFlagService.IsEnabledAsync()` in handlers.

## Project Structure

```
src/
├── API/                    # API hosts (main + module-specific)
├── Common/                 # Shared: Domain, Application, Infrastructure, Presentation
└── Modules/                # Customer, Fees, Organization, Product, Sales, SampleOrders, SampleSales
```

**Layer Dependencies**: Domain -> Application -> Infrastructure/Presentation -> API

## Multi-API Architecture

| API Host | Modules |
|----------|---------|
| `Rtl.Core.Api` | All (monolith) |
| `Rtl.Core.Api.Customer` | Customer only |
| `Rtl.Core.Api.Fees` | Fees only |
| `Rtl.Core.Api.Organization` | Organization only |
| `Rtl.Core.Api.Product` | Product only |
| `Rtl.Core.Api.Sales` | Sales only |
| `Rtl.Core.Api.SampleOrders` | SampleOrders only |
| `Rtl.Core.Api.SampleSales` | SampleSales only |

Enables independent scaling, targeted deployments, and gradual microservice extraction.

## Getting Started

**Prerequisites**: .NET 10 SDK, PostgreSQL, Redis (optional)

**Configure** `appsettings.json` with your `ConnectionStrings:Database` and `ConnectionStrings:Cache`.

```bash
dotnet run --project src/Api/Host   # API at https://localhost:5001
dotnet test                                         # Run all tests
```

## EF Core Migrations

Each module has its own DbContext. Migrations auto-apply on startup.

| Module | DbContext |
|--------|-----------|
| Customer | `CustomerDbContext` |
| Fees | `FeesDbContext` |
| Organization | `OrganizationDbContext` |
| Product | `ProductDbContext` |
| Sales | `SalesDbContext` |
| SampleOrders | `OrdersDbContext` |
| SampleSales | `SampleDbContext` |

**Command pattern** (replace `{Module}` and `{DbContext}`):

```bash
# Add migration
dotnet ef migrations add <Name> \
  --project src/Modules/{Module}/Infrastructure \
  --startup-project src/Api/Host \
  --context {DbContext} \
  --output-dir Persistence/Migrations

# Remove last migration
dotnet ef migrations remove \
  --project src/Modules/{Module}/Infrastructure \
  --startup-project src/Api/Host \
  --context {DbContext}

# Apply manually
dotnet ef database update \
  --project src/Modules/{Module}/Infrastructure \
  --startup-project src/Api/Host \
  --context {DbContext}
```

## Code Coverage

Coverage collected via Coverlet on PR with ReportGenerator for detailed reports.

**PR Comments include:**
- Summary with class/method breakdown
- Delta summary (coverage changes from previous run)
- Risk hotspots (complex code with low coverage)

**Artifacts:** Download `coverage-report-html` from Actions for interactive drill-down to line level.

```bash
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
```

## Creating a New Module

1. Create folder structure under `src/Modules/YourModule/` with Domain, Application, Infrastructure, Presentation projects
2. Add project references following layer dependencies
3. Create domain entities (extend `Entity`, use `Raise()` for domain events)
4. Create commands (`ICommand<T>`) and queries (`IQuery<T>`)
5. Create handlers (`ICommandHandler<,>` / `IQueryHandler<,>`)
6. Create endpoints (`IEndpoint`)
7. Register in `Program.cs`: `builder.Services.AddYourModule(builder.Configuration);`

## Patterns

- **Result Pattern** - `Result.Success()` / `Result.Failure()` for error handling
- **Domain Events** - `entity.Raise(new DomainEvent())` published via outbox
- **CQRS** - `ICommand<T>` / `IQuery<T>` with MediatR
- **Pipeline Behaviors** - Validation, Logging, Exception handling

## Tech Stack

.NET 10, EF Core 9 (PostgreSQL), Dapper, MediatR, FluentValidation, AWS EventBridge/SQS, Quartz.NET, Redis, OpenTelemetry, Serilog, Swagger
