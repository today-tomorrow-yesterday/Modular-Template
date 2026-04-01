---
name: verification-pipeline
description: Use after completing implementation work to verify everything is correct before committing - runs build, tests, and convention checks in sequence
---

# Verification Pipeline

Run this pipeline after completing any implementation work. Each phase must pass before proceeding to the next.

## Phase 1: Build

```bash
dotnet build
```

Zero errors, zero warnings (where possible). Fix all `CS` errors before continuing.

## Phase 2: Run Unit Tests

```bash
dotnet test --filter "FullyQualifiedName~Domain.Tests|FullyQualifiedName~Application.Tests" --no-build -v q
```

All domain and application tests must pass. If tests fail, fix the implementation — do not modify tests to make them pass unless the test itself is wrong.

## Phase 3: Run Integration Tests (if available)

```bash
dotnet test --filter "FullyQualifiedName~Integration" --no-build -v q
```

Requires database connection. Skip if no integration test infrastructure exists yet.

## Phase 4: Convention Check

Manually verify these — they cause subtle bugs that the compiler won't catch:

- [ ] **No `int Id` in integration events** — grep for `int.*Id,` in IntegrationEvents/ folders
- [ ] **No `Guid.NewGuid()`** — should be `Guid.CreateVersion7()` everywhere
- [ ] **All if statements have curly braces** — no braceless single-line returns
- [ ] **Cache entities use `RefPublicId`** — not `RefPublic{Entity}Id`
- [ ] **Upserts match on `RefPublicId`** — not `FindAsync` by int Id
- [ ] **`[SensitiveData]` on financial/PII fields** — prices, addresses, DOB, phone, email
- [ ] **Every endpoint has `.WithName()`** — Swagger breaks without it

Quick grep commands:
```bash
# Find int IDs leaking into integration events
grep -rn "int.*Id," src/Modules/*/IntegrationEvents/ --include="*.cs"

# Find Guid.NewGuid() (should be CreateVersion7)
grep -rn "Guid.NewGuid" src/Modules/ --include="*.cs" | grep -v test | grep -v Migration

# Find braceless if statements
grep -Pn "^\s+if\s*\(.*\)\s*$" src/Modules/*/Domain/ --include="*.cs" -A1 | grep -v "{"
```

## Phase 5: Domain Event Audit

For any new entities or behavioral methods:
- [ ] Factory methods raise creation events
- [ ] State-changing methods raise appropriate domain events
- [ ] Domain events that cross modules have corresponding integration event handlers
- [ ] Integration event handlers map `PublicId` (never `Id`)

## When to Run

- **Full pipeline (1-5):** Before committing feature work
- **Phases 1-2 only:** After refactoring or renaming
- **Phase 4 only:** During code review

## Checklist

- [ ] `dotnet build` — zero errors
- [ ] Domain tests pass
- [ ] Application tests pass
- [ ] Integration tests pass (if available)
- [ ] No convention violations found
- [ ] Domain events properly wired
