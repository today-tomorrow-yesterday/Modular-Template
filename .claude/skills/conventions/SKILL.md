---
name: mm-conventions
description: Use when writing any code in this modular monolith - enforces naming conventions, ID patterns, and cross-module contract rules
---

# Modular Monolith Conventions

## Cross-Module Identity Rules

**NEVER expose `int Id` across module boundaries.** Internal entity IDs (`int Id` from `Entity` base class) are private to the owning module. Cross-module references always use `Guid PublicId`.

### Integration Events
- Identity property: `Guid Public{EntityName}Id` (e.g., `PublicCustomerId`, `PublicOnLotHomeId`, `PublicLandParcelId`, `PublicUserId`)
- NEVER include `int` entity IDs in integration events
- Add `[EventDetailType("rtl.{module}.{camelCaseEventName}")]` attribute
- Base: `IntegrationEvent(Id, OccurredOnUtc)` where `Id` = `Guid.CreateVersion7()`
- Remove events carry ONLY the `Guid Public{Entity}Id` — lean payload, nothing else

### API Response DTOs
- Use `Guid PublicId` — NEVER `int Id`
- Drop "Ref" prefix from properties (Ref is internal domain naming)

### Cache Entities in Consumer Modules
- Cross-module reference property: `Guid RefPublicId` (short form, matches `CustomerCache.RefPublicId`)
- NOT `RefPublic{EntityName}Id` — the entity class name already provides context
- Internal `int Id` is auto-generated (`UseIdentityAlwaysColumn`) — this is the Sales module's own key

## Entity Classification

| Pattern | Base Class | When to Use |
|---------|-----------|-------------|
| Source of truth (module owns this data) | `Entity, IAggregateRoot` | Customer, Sale, Package |
| CDC cache with change detection | `Entity, ICacheProjection` | OnLotHome, LandParcel (emits domain events on data changes) |
| Read-only cache (no events) | `ICacheProjection` only | LandCost, AncillaryData, WheelsAndAxlesTransaction |
| Cross-module cache in consumer | `Entity, ICacheProjection` | OnLotHomeCache, LandParcelCache (extends Entity for domain events like price change detection) |
| Simple cross-module cache | `ICacheProjection` only | CustomerCache, AuthorizedUserCache |

## Property Naming

- `PublicId` — Guid v7, generated in `Create()` factory method, unique indexed
- `Ref{PropertyName}` — properties that reference external system data (e.g., `RefHomeCenterNumber`, `RefStockNumber` from iSeries)
- `RefPublicId` — Guid referencing another module's entity PublicId (on cache entities)
- `LastSyncedAtUtc` — required on all `ICacheProjection` entities

## Domain Events

- Aggregate root events carry data needed by handlers (handlers load entity for full state)
- Remove events carry `Guid PublicId` (entity may be gone when handler runs)
- Add/Revise events carry no payload — handlers load the entity from the repository
- Event naming: `{Entity}{Action}DomainEvent` (e.g., `OnLotHomeAddedDomainEvent`, `LandParcelRemovedDomainEvent`)

## EF Configuration Patterns

- Column names: `snake_case` (e.g., `public_id`, `ref_home_center_number`)
- Index names: `ix_{table}_{column}` (e.g., `ix_on_lot_homes_public_id`)
- Cache tables: schema `cache`, table name `{entity}_cache`
- `PublicId`: always unique indexed
- Composite business key `(RefHomeCenterNumber, RefStockNumber)`: always unique indexed

## Swagger / Minimal API

- Every endpoint MUST have `.WithName("UniqueOperationId")` for Swagger to distinguish them
- Without `.WithName()`, endpoints sharing `HandleAsync` get the same `operationId`

## Sensitive Data

- Mark financial fields with `[SensitiveData]`: prices, costs, invoice amounts, appraisals
- Mark PII with `[SensitiveData]`: addresses, phone numbers, names, SSN, DOB, serial numbers, loan numbers
- AES-256-GCM encryption via `[SensitiveData]` attribute — applied automatically by EF value converter
