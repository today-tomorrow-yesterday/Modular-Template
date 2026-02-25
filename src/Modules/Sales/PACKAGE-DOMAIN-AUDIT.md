# Package Domain Module Audit

**Date:** 2026-02-25
**Scope:** `src/Modules/Sales/Domain/Packages/`, Application handlers, Infrastructure repository, Tests
**Method:** 6 parallel AI agents auditing entity methods, line types, details classes, removal methods, repository implementation, and test coverage

---

## CRITICAL (7)

### C-1: `UpdateDeliveryAddressCommandHandler` was mutating no-tracking entities (FIXED)

**File:** `Application/DeliveryAddresses/UpdateDeliveryAddress/UpdateDeliveryAddressCommandHandler.cs`

The handler called `GetBySaleIdAsync()` (no-tracking) then mutated the returned packages — `RemoveHomeFirstInsuranceLine()`, `RemoveWarrantyLine()`, `ClearQuestionAnswers()`, `FlagForTaxRecalculation()`, etc. None of those changes were persisted by `SaveChangesAsync` because the entities were detached from the change tracker.

**Impact:** Silent data loss on every delivery address update that triggers package side-effects.

**Status:** Fixed — changed to `GetBySaleIdWithTrackingAsync()`.

---

### C-2: `RecalculateGrossProfit()` unconditionally overwrites `CommissionableGrossProfit`

**File:** `Domain/Packages/Package.cs` — `RecalculateGrossProfit()` (private)

```csharp
private void RecalculateGrossProfit()
{
    var grossProfit = _lines
        .Where(l => !l.ShouldExcludeFromPricing)
        .Sum(l => l.SalePrice - l.EstimatedCost);

    GrossProfit = grossProfit;
    CommissionableGrossProfit = grossProfit; // <-- destroys commission-calculated value
}
```

`CommissionableGrossProfit` is computed by `CalculateCommissionCommandHandler` via `package.SetCommissionableGrossProfit()`. That value is typically different from `GrossProfit` because the commission engine excludes certain line items. However, every `AddLine`/`RemoveLine` call resets it back to raw `GrossProfit`.

**Impact:** API returns incorrect `CommissionableGrossProfit` after any line mutation until commission is recalculated. Stale value persisted to database.

**Suggested fix:** Stop overwriting `CommissionableGrossProfit` in `RecalculateGrossProfit()`. Let only the commission handler set it. Consider adding a `MustRecalculateCommission` flag analogous to `MustRecalculateTaxes`.

---

### C-3: `UpdatePricing()` on an attached line does NOT trigger `RecalculateGrossProfit()`

**File:** `Domain/Packages/PackageLine.cs` — `UpdatePricing()`

`PackageLine.UpdatePricing()` modifies `SalePrice`, `EstimatedCost`, and `RetailSalePrice` but has no back-reference to the owning `Package` to trigger GP recalculation.

**Affected caller:** `UpdatePackageLandCommandHandler.RecalculateLandPricing()` calls `landLine.UpdatePricing(salePrice, estimatedCost, ...)` after the line is already added to the package. The `AddLine()` that preceded it already triggered `RecalculateGrossProfit()` with the original values, but the subsequent `UpdatePricing()` overwrites those values without recalculating.

**Impact:** `GrossProfit` is stale (based on pre-recalculation prices) and persisted to the database.

**Suggested fix:** Either (a) expose `RecalculateGrossProfit()` as internal and call it after `UpdatePricing`, or (b) create `Package.UpdateLinePricing(line, ...)` that both updates the line and recalculates GP, or (c) restructure to compute final prices before `AddLine`.

---

### C-4: `RemoveInsuranceLine()` throws when multiple insurance lines exist

**File:** `Domain/Packages/Package.cs` — `RemoveInsuranceLine()`

```csharp
public InsuranceLine? RemoveInsuranceLine()
{
    var line = _lines
        .OfType<InsuranceLine>()
        .SingleOrDefault(); // throws if >1 match
    ...
}
```

`InsuranceLine` is classified as 1:many in `PackageLineTypeConstants.OneToManyTypes`. A package can have both HomeFirst and Outside insurance lines simultaneously. `SingleOrDefault()` throws `InvalidOperationException` when both exist.

**Affected callers:**
- `UpdatePackageInsuranceCommandHandler` (line 50)
- `RecordOutsideInsuranceCommandHandler` (line 47)

**Impact:** Unhandled 500 error when updating insurance while both types coexist.

**Suggested fix:** Split into `RemoveOutsideInsuranceLine()` that filters by `InsuranceType.Outside`, or change to `FirstOrDefault()`. Callers should use type-specific removal methods.

---

### C-5: `TaxDetails.With*()` methods share list references between old and new instances

**File:** `Domain/Packages/Details/TaxDetails.cs`

`WithClearedQuestionAnswers()`, `WithClearedCalculations()`, `WithClearedErrors()`, and `WithClearedPreviouslyTitled()` create new `TaxDetails` instances but copy `StateTaxQuestionAnswers` and/or `Taxes` lists by reference. Both old and new instances point to the same `List<T>`. Mutating one mutates the other.

**Impact:** If any downstream code adds/removes items from the list on one instance, the other instance is silently corrupted.

**Suggested fix:** Use `.ToList()` when copying lists into new instances:
```csharp
StateTaxQuestionAnswers = StateTaxQuestionAnswers.ToList(),
Taxes = Taxes.ToList(),
```

Also apply defensive copies in `Create()`:
```csharp
StateTaxQuestionAnswers = questionAnswers.ToList(),
Taxes = taxes.ToList(),
Errors = errors?.ToList()
```

---

### C-6: `SalesTeamDetails.Create()` aliases the caller's list

**File:** `Domain/Packages/Details/SalesTeamDetails.cs`

```csharp
SalesTeamMembers = members // stores caller's list directly
```

The caller retains a mutable reference to internal state. Any external `.Add()`, `.Remove()`, or `.Clear()` silently mutates the details object.

**Suggested fix:** `SalesTeamMembers = members.ToList()`

---

### C-7: `Package.Version` concurrency token is non-functional

**File:** `Infrastructure/Persistence/Configurations/PackageConfiguration.cs`

The `Package` entity has `int? Version` with comment "Optimistic concurrency", but:
1. No `.IsConcurrencyToken()` or `.IsRowVersion()` in EF config
2. No `IncrementVersion()` or setter — the property is never written

**Impact:** Concurrent writes to the same package silently overwrite each other. No `DbUpdateConcurrencyException` is ever thrown.

**Suggested fix:** Add `.IsConcurrencyToken()` to the configuration and implement version incrementing (or use PostgreSQL `xmin` column approach).

---

## WARNING (22)

### Line Types / Constants

| # | Finding | File |
|---|---------|------|
| W-1 | `Discount` and `Fee` declared in `PackageLineTypeConstants.ValidTypes` and `OneToManyTypes` but no concrete classes or EF discriminator mappings exist. If legacy data has these discriminators, EF Core will fail to materialize rows. | `Domain/Packages/PackageLineTypeConstants.cs` |

### Details Classes

| # | Finding | File |
|---|---------|------|
| W-2 | `TaxDetails.StateTaxQuestionAnswers` and `Taxes` typed as `List<T>` with `private set` — externally mutable via `.Add()/.Clear()`. Should be `IReadOnlyList<T>`. | `Details/TaxDetails.cs` |
| W-3 | `TaxDetails.Errors` is `List<string>?` — nullable AND mutable, inconsistent with other lists defaulting to `[]`. | `Details/TaxDetails.cs` |
| W-4 | `TaxDetails.Create()` stores caller's list references without defensive `.ToList()` copy. | `Details/TaxDetails.cs` |
| W-5 | `TaxDetails`, `InsuranceDetails`, `LandDetails`, `SalesTeamDetails` missing private parameterless constructors — allows `new TaxDetails()` bypassing the factory. | Multiple files |
| W-6 | `InsuranceDetails.Create()` and `WarrantyDetails.Create()` hardcode `QuotedAt = UtcNow` with no parameter override — impossible to reconstruct with original timestamp in tests/migrations. | `Details/InsuranceDetails.cs`, `Details/WarrantyDetails.cs` |
| W-7 | `HomeDetails.SerialNumbers` is `string[]?` — mutable array contents, no defensive `.ToArray()` copy in factory. | `Details/HomeDetails.cs` |
| W-8 | `CreditDetails` uses public constructor instead of static `Create()` factory — only details class that breaks the pattern. | `Details/CreditDetails.cs` |

### Package Entity

| # | Finding | File |
|---|---------|------|
| W-9 | `Package.Create` sets `Ranking = 0` for non-primary packages — semantically undefined. Comment says "1 = primary, 2+ = alternates" but 0 is neither. | `Package.cs` |
| W-10 | `SetName(string)` has no null/empty/whitespace validation or trimming. Entity provides no invariant protection. | `Package.cs` |
| W-11 | `SetNonPrimary(int)` accepts any integer including 0, 1, or negative — no guard clause. `SetNonPrimary(1)` would silently make a package appear primary. | `Package.cs` |
| W-12 | `RemoveLine()` does not raise `HomeLineUpdatedDomainEvent` / `LandLineUpdatedDomainEvent` — asymmetric with `AddLine()`. A standalone removal without replacement won't notify insurance/warranty/tax handlers. | `Package.cs` |
| W-13 | `SalesTeamLine.ShouldExcludeFromPricing` relies on implicit `bool` default (`false`) instead of explicit `override => true`. If base class default changes, SalesTeamLine silently changes behavior. All prices are 0m so it's harmless today. | `Lines/SalesTeamLine.cs` |

### Application Handlers

| # | Finding | Files |
|---|---------|-------|
| W-14 | 7 handlers use generic `RemoveLine(existing)` instead of typed methods: `UpdatePackageWarranty`, `GenerateWarrantyQuote`, `UpdatePackageTax`, `CalculateTaxes` (2 locations), `UpdatePackageLand`, `UpdatePackageHome`, `UpdatePackageDownPayment`. If typed methods gain additional behavior (events, validation), these callers won't pick it up. | Multiple handlers |
| W-15 | `RecalculateGrossProfit()` called N times in loops — `UpdatePackageProjectCosts` removes user-managed PCs one-by-one, and `UpdatePackageHome.RemoveInvalidProjectCosts` removes up to 9 individually. Each triggers a full GP recalculation. | `UpdatePackageProjectCostsCommandHandler.cs`, `UpdatePackageHomeCommandHandler.cs` |

### Repository / Infrastructure

| # | Finding | File |
|---|---------|------|
| W-16 | `GetByIdAsync` override silently changes tracking behavior from base class contract (base uses `AsNoTracking`, override is tracked). No current callers, but violates interface contract. | `Repositories/PackageRepository.cs` |
| W-17 | `SetPackageAsPrimaryCommandHandler` loads target package twice — first by PublicId, then all-by-SaleId (which includes target again). Redundant DB round-trip. | `SetPackageAsPrimaryCommandHandler.cs` |
| W-18 | TOCTOU race on duplicate package name check — `CreatePackageCommandHandler` and `UpdatePackageNameCommandHandler` check in-memory after loading no-tracking. No unique index on `(sale_id, lower(name))`. Concurrent requests could create duplicates. | `CreatePackageCommandHandler.cs`, `UpdatePackageNameCommandHandler.cs` |
| W-19 | `GetAllAsync` inherited from base `ReadRepository` lacks `.Include(p => p.Lines)` — returns packages with empty Lines. Available on interface but broken. | `ReadRepository.cs` |

### Tests

| # | Finding | File |
|---|---------|------|
| W-20 | No test for `RemoveInsuranceLine()` with 2 coexisting insurance lines — would have caught C-4 immediately. | `PackageLineRemovalTests.cs` |
| W-21 | GP recalculation not tested for `RemoveHomeLine`, `RemoveLandLine`, `RemoveTaxLine`, `RemoveInsuranceLine`, `RemoveSalesTeamLine` — only `RemoveWarrantyLine` and `RemoveProjectCost` have GP assertions. | `PackageLineRemovalTests.cs` |
| W-22 | `Package.Create(isPrimary: false)` never tested at domain level — every test uses `isPrimary: true`. | All domain tests |

---

## INFO (16)

### Details Classes

| # | Finding |
|---|---------|
| I-1 | `TaxItem.CalculatedAmount` is `decimal?` but factory always provides non-null. Nullability only relevant for schema evolution during deserialization. |
| I-2 | `TradeInDetails.Create()` applies `Math.Round(..., 2)` to monetary fields at construction time — silent rounding could mask upstream precision issues. |
| I-3 | `TaxDetails.With*()` methods do not propagate `ExtensionData` — creating a cleared copy drops extension data. Low risk during dev, potential data loss after schema upgrades. |
| I-4 | `InsuranceType.Warranty` enum value exists but is never used anywhere. Warranty products use `WarrantyLine`, not `InsuranceLine` with `InsuranceType.Warranty`. Likely a legacy holdover. |

### Line Types

| # | Finding |
|---|---------|
| I-5 | `TaxLine` does not set `Responsibility` — remains null. Intentional for tax calculations. |
| I-6 | `CreditLine` does not accept `SortOrder` parameter despite being 1:many. Multiple credits of same subtype all get `SortOrder = 0`. |
| I-7 | `TaxLine.ClearCalculations()` uses `SalePrice = 0` (int literal) instead of `0m`. No runtime impact, cosmetic inconsistency. |
| I-8 | `TaxItem` amounts not rounded at creation — protected by rounding at `TaxLine.SalePrice` level. |

### Package Entity

| # | Finding |
|---|---------|
| I-9 | `PackageStatus` enum only has `Draft`. Guard code for non-Draft deletion is unreachable. Scaffolding for future statuses. |
| I-10 | `Package.Create` raises `PackageReadyForFundingDomainEvent` with mostly-empty data (`RequestAmount = 0m`, missing `SalePublicId`, `CustomerId`, etc.). Handler re-loads everything anyway. |
| I-11 | `AddLine` does not enforce 1:1 constraints — callers must do remove-then-add. If a caller forgets the remove, package silently gets duplicates that crash `SingleOrDefault()`. |
| I-12 | `RemoveHomeLine`, `RemoveLandLine`, `RemoveTaxLine`, `RemoveDownPaymentLine` have zero production callers — only used in tests. Not dead code but unused convenience methods. |

### Test Coverage

| # | Finding |
|---|---------|
| I-13 | 7 of 23 public Package methods have zero test coverage: `FlagForTaxRecalculation`, `ClearTaxRecalculationFlag`, `SetCommissionableGrossProfit`, `Package.Create` (dedicated), domain events from `Create`/`AddLine(Home)`/`AddLine(Land)`. |
| I-14 | All 4 `TaxLine` mutation methods untested: `ClearQuestionAnswers`, `ClearCalculations`, `ClearErrors`, `ClearPreviouslyTitled`. |
| I-15 | `HomeLine.DetachProduct()` and `LandLine.DetachProduct()` untested. |
| I-16 | No multi-line GP integration test combining Home + Land + ProjectCost + Credit + TradeIn to verify the formula with mixed `ShouldExcludeFromPricing` values. GP assertions only test single-line-type packages. |

---

## Priority Actions

### Immediate (data correctness)

1. **C-4:** Fix `RemoveInsuranceLine()` — change to type-filtered removal or split into `RemoveOutsideInsuranceLine()`
2. **C-2:** Stop clobbering `CommissionableGrossProfit` in `RecalculateGrossProfit()`
3. **C-3:** Ensure `RecalculateGrossProfit()` runs after `UpdatePricing()` on attached lines

### Short-term (defensive correctness)

4. **C-5/C-6:** Add `.ToList()` defensive copies in `TaxDetails` and `SalesTeamDetails`
5. **C-7:** Configure `Version` as a concurrency token or remove the dead property
6. **W-18:** Add unique index `(sale_id, lower(name))` to prevent duplicate package names

### Medium-term (code quality)

7. **W-14:** Migrate 7 handlers from generic `RemoveLine` to typed removal methods
8. **W-10/W-11:** Add guard clauses to `SetName` and `SetNonPrimary`
9. **W-12:** Add domain event raising to `RemoveLine` for Home/Land lines
10. **I-13/I-14:** Fill critical test coverage gaps (domain events, TaxLine mutations, `Package.Create`)
