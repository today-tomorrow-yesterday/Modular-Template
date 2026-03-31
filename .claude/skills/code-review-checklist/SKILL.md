---
name: mm-code-review
description: Use when reviewing code changes or PRs in this modular monolith - project-specific checklist covering ID leaks, naming conventions, cross-module boundaries, and common mistakes
---

# Modular Monolith Code Review Checklist

Project-specific review items beyond general code quality. These are the mistakes we've actually made and fixed — check for them on every review.

## Critical: Cross-Module Boundary Violations

- [ ] **No `int Id` in integration events** — only `Guid Public{EntityName}Id`
- [ ] **No `int Id` in API response DTOs** — only `Guid PublicId`
- [ ] **No direct domain/application references between modules** — only IntegrationEvents project references
- [ ] **Integration events don't use "Ref" prefix** — that's internal domain naming
- [ ] **Remove events carry ONLY the Guid** — lean payload, no StockNumber/HomeCenterNumber

## Naming Conventions

- [ ] **Integration event identity:** `Guid Public{EntityName}Id` (e.g., `PublicCustomerId`, `PublicOnLotHomeId`)
- [ ] **Cache entity cross-ref:** `Guid RefPublicId` (short form on cache entities)
- [ ] **Domain "Ref" prefix:** Only on properties referencing external system data (`RefHomeCenterNumber`, `RefStockNumber`)
- [ ] **Event detail type:** `[EventDetailType("rtl.{module}.{camelCaseEventName}")]`
- [ ] **EF columns:** snake_case (`ref_home_center_number`, `public_id`)
- [ ] **EF index names:** `ix_{table}_{column}` pattern
- [ ] **Error codes:** `{Entities}.{ErrorName}` (plural prefix)

## Entity Classification

- [ ] **Aggregate roots** use `Entity, IAggregateRoot` — module owns the data
- [ ] **CDC cache entities** use `Entity, ICacheProjection` — external data with domain events
- [ ] **Simple cache** implements only `ICacheProjection` — no domain events needed
- [ ] **Cache entities in consumer modules** use `Entity, ICacheProjection` if they need change detection events

## Swagger / API

- [ ] **Every endpoint has `.WithName("UniqueOperationId")`** — Swagger breaks without it
- [ ] **Endpoints use `.WithTags()`** for grouping
- [ ] **Response wrapped in `ApiEnvelope<T>`**
- [ ] **Errors return ProblemDetails** via `.ToProblem()`

## EF Core

- [ ] **JSONB uses `VersionedJsonConverter<T>`** — not `OwnsOne`/`ToJson`
- [ ] **Details classes implement `IVersionedDetails`** with `SchemaVersion` + `ExtensionData`
- [ ] **`[SensitiveData]` on financial and PII fields** — prices, addresses, SSN, DOB, serial numbers
- [ ] **`DomainEvents` ignored** in EF config for Entity-derived classes
- [ ] **Correct PK strategy** — HiLo for owned, ValueGeneratedNever for CDC, UseIdentityAlwaysColumn for cache

## Domain

- [ ] **Factory methods return entity** — no public constructors
- [ ] **Behavioral methods return `Result<T>`** — no exceptions for domain logic
- [ ] **`PublicId = Guid.CreateVersion7()`** in factory method
- [ ] **Domain events raised** in factory and behavioral methods
- [ ] **Collections exposed as `IReadOnlyCollection<T>`** via `.AsReadOnly()`

## Testing

- [ ] **Integration tests arrange via commands** — not direct DB inserts
- [ ] **Cache seeding uses `ICacheWriteScope.AllowWrites()`** in using block
- [ ] **Respawner includes correct schemas** — don't forget "messaging" for event tests
- [ ] **Event producer tests use SpyEventBus** + `Spy.Clear()` between phases
- [ ] **Event consumer tests reset both source and consumer databases**

## Common Mistakes We've Fixed

These are real bugs we've caught in this codebase:

1. **Leaking `int Id` across module boundaries** — integration events sending iSeries CDC keys
2. **`RefPublic{EntityName}Id` instead of `RefPublicId`** on cache entities — too verbose
3. **Missing `.WithName()` on endpoints** — causes Swagger to group unrelated endpoints
4. **`IAggregateRoot` on CDC cache entities** — they don't own the data, should be `ICacheProjection`
5. **`SaleSummaryCache` in Inventory** — Inventory shouldn't cache Sales data (removed)
6. **`CrmPartyId` naming** — renamed to `CrmCustomerId` to match domain language
7. **`private set` on `LastSyncedAtUtc`** for `ICacheProjection` — interface requires public setter
8. **Redundant `int SaleId` + `Guid SalePublicId`** on DeliveryAddress events — just the Guid
9. **`int UserId` + `Guid PublicId`** on Organization events — drop the int, rename to `PublicUserId`
