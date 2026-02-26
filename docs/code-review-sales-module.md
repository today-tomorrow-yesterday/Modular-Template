# Sales Module — Code Review Findings

> **Date:** 2026-02-25
> **Scope:** Command handlers in `src/Modules/Sales/Application/`, iSeries adapter, SaleRepository
> **Methodology:** Compared new handlers against legacy `UpsertPackageCommandHandler` (905 lines) and `PackageDtoExtensions` (760 lines) from `rtl-domain-sales-api`

---

## Status Legend

| Status | Meaning |
|--------|---------|
| FIXED | Already resolved in this review session |
| OPEN | Needs work |
| ACCEPTED | Matches legacy behavior; not a regression |

---

## 1. Production Bug — Missing Party Include (FIXED)

**File:** `src/Modules/Sales/Infrastructure/Persistence/Repositories/SaleRepository.cs:34-41`

`GetByPublicIdWithFullContextAsync` did not include `.Include(s => s.Party).ThenInclude(p => p.Person)`, but `GenerateHomeFirstQuoteCommandHandler` accesses `ctx.Sale.Party.Person.FirstName` (line 135). Tests passed because Party was set via reflection. Production would NRE on the HomeFirst quote flow.

**Fix applied:** Added `.Include(s => s.Party).ThenInclude(p => p.Person)` to the query.

---

## 2. Superficial Validation — Non-Nullable Validated State (FIXED)

**Files:**
- `src/Modules/Sales/Application/Insurance/GenerateHomeFirstQuote/GenerateHomeFirstQuoteCommandHandler.cs`
- `src/Modules/Sales/Application/Insurance/GenerateWarrantyQuote/GenerateWarrantyQuoteCommandHandler.cs`

Both handlers validated `DeliveryAddress` early but kept it nullable in `ValidatedSaleContext`, forcing every downstream reference to use `?.` / `??` even after validation. The HomeFirst handler was missing the `DeliveryAddress is null` guard entirely (unlike the warranty handler which had one).

**Fix applied:**
- Added `DeliveryAddress is null` guard to HomeFirst handler
- Made `ValidatedSaleContext.DeliveryAddress` non-nullable in both handlers
- Removed 8+ redundant `?.` operators and `!` null-forgiving operators across both handlers

---

## 3. W&A Pricing — EstimatedCost vs SalePrice Collapse (FIXED)

**Files:**
- `src/Common/Application/Adapters/ISeries/IiSeriesAdapter.cs`
- `src/Common/Infrastructure/ISeries/iSeriesAdapter.cs`
- `src/Common/Application/Adapters/ISeries/Pricing/WheelAndAxlePriceResult.cs` (new)
- `src/Modules/Sales/Application/Packages/UpdatePackageHome/UpdatePackageHomeCommandHandler.cs`
- `src/Modules/Sales/Application/Pricing/GetWheelsAndAxlesPrice/GetWheelsAndAxlesPriceQueryHandler.cs`
- `src/Modules/Sales/Application/Pricing/GetWheelsAndAxlesPriceByStock/GetWheelsAndAxlesPriceByStockQueryHandler.cs`
- `src/Api/Host/Diagnostics/ISeriesDiagnosticsEndpoints.cs`

The iSeries ByCount endpoint (`v1/wheels-and-axles/price`) returns both `SalePrice` and `Cost`, but the adapter discarded `Cost` and returned a single `decimal`. The handler used that single value for all three package line fields (`salePrice`, `estimatedCost`, `retailSalePrice`). Legacy used `pricing.Cost` for `estimatedCost` and `pricing.SalePrice` for the other two.

**Fix applied:**
- Created `WheelAndAxlePriceResult(decimal SalePrice, decimal Cost)` result type
- Changed both adapter methods from `Task<decimal>` to `Task<WheelAndAxlePriceResult>`
- ByStock: returns `(price, price)` — wire only has one value
- ByCount: returns `(result.SalePrice, result.Cost)` — preserves both
- Handler now sets `estimatedCost: waResult.Cost`, `salePrice: waResult.SalePrice`, `retailSalePrice: waResult.SalePrice`

---

## 4. Use Tax Magic Numbers — Bare `9, 21` in Two Handlers (FIXED)

**Files:**
- `src/Modules/Sales/Application/Packages/UpdatePackageWarranty/UpdatePackageWarrantyCommandHandler.cs:49`
- `src/Modules/Sales/Application/Insurance/GenerateWarrantyQuote/GenerateWarrantyQuoteCommandHandler.cs:161`

Both warranty-related handlers used bare `package.RemoveProjectCost(9, 21)` instead of named constants.

**Fix applied:** Both handlers now use `ProjectCostCategories/ProjectCostItems.UseTaxCategoryNumber` / `UseTaxItemNumber` from the shared domain class.

---

## 5. Use Tax Constants Duplicated Across 8+ Handlers (FIXED)

**Files (each has its own `private const` declaration):**
- `UpdatePackageHomeCommandHandler.cs:313-314`
- `UpdatePackageConcessionsCommandHandler.cs:23-24`
- `UpdatePackageTradeInsCommandHandler.cs:23-24`
- `UpdatePackageProjectCostsCommandHandler.cs:32-33`
- `UpdatePackageLandCommandHandler.cs:22-23`
- `UpdateDeliveryAddressCommandHandler.cs:21-22`
- `CalculateTaxesCommandHandler.cs:27-28`
- `UpdatePackageTaxCommandHandler.cs:22-23`

`UseTaxCategoryNumber = 9` and `UseTaxItemNumber = 21` were declared independently in every handler. The two warranty handlers (finding #4) skipped the constants entirely. This was 10 total call sites for the same well-known natural key.

**Fix applied:** Extracted to `ProjectCostCategories/ProjectCostItems` in `Modules.Sales.Domain.Packages`. Removed all 8 `private const` / `internal const` declarations and updated all 10 call sites (including the 2 bare `9, 21` calls from finding #4) to reference the shared class.

---

## 6. W&A Description Strings Changed from Legacy (ACCEPTED)

**File:** `src/Modules/Sales/Application/Packages/UpdatePackageHome/UpdatePackageHomeCommandHandler.cs:234-236`

| Legacy | New |
|--------|-----|
| `"W&A Rental Cost"` | `"Wheels & Axles - Rental"` |
| `"W&A Sales"` | `"Wheels & Axles - Purchase"` |

These description strings are stored in `ProjectCostDetails.ItemDescription` and may surface in UI or reports. The change is cosmetic but intentional — the new descriptions are clearer. If legacy string values matter for reporting continuity, these would need to match. Otherwise, accepted.

---

## 7. Tax Change Detection — Snapshot Pattern Inconsistency (ACCEPTED)

The 7 pricing-mutation handlers all follow the same capture-mutate-compare-flag lifecycle, but use 3 different structural approaches:

| Approach | Handlers | Example |
|----------|----------|---------|
| Sealed record + named methods | UpdatePackageHome, GenerateWarrantyQuote | `TaxSnapshot` record, `TakeTaxSnapshot()`, `DetectTaxChanges()` |
| Inline variables + static method | UpdatePackageTradeIns, UpdatePackageProjectCosts | 2-3 local vars + `DetectTaxChanges()` static method |
| Inline variables + inline comparison | UpdatePackageLand, UpdatePackageConcessions, UpdatePackageWarranty | 1-2 local vars + inline `if` block |

The inconsistency is cosmetic, not functional. Each handler's snapshot complexity matches its detection needs: UpdatePackageHome tracks 5 fields (justifies a record), concessions tracks 1 field (a record would be over-engineering). The action taken on change is identical everywhere: `ClearCalculations()` -> `RemoveProjectCost(UseTax)` -> `FlagForTaxRecalculation()`.

Not a bug. Accepted as reasonable variation.

---

## 8. Tax Detection — Legacy Early Exit Guard Removed (ACCEPTED)

Legacy `HandleTaxCalculation` skipped entirely if `taxPackageLine?.Details?.Taxes is null || .Count == 0` — meaning if taxes were never calculated yet, inputs could change without flagging recalculation. The new handlers don't have this guard; they always evaluate and flag.

This is more correct. A first-time home price change should flag "needs tax calculation" even if taxes haven't been calculated yet. Accepted as an improvement.

---

## 9. Legacy Comparison — `HandleTaxCalculation` Decomposition (ACCEPTED)

The legacy monolithic `HandleTaxCalculation` compared 9 fields in one shot. The new system decomposes this across 7 handlers, each responsible for its own domain:

| Legacy Field | New Handler |
|---|---|
| homeTypeChanged | UpdatePackageHome |
| homeStockNumberChanged | UpdatePackageHome |
| homeSalePriceChanged | UpdatePackageHome |
| landSalePriceChanged | UpdatePackageLand |
| projectCostCountChanged | UpdatePackageHome, UpdatePackageProjectCosts, UpdatePackageTradeIns |
| projectCostSalePriceChanged | UpdatePackageHome, UpdatePackageProjectCosts, UpdatePackageTradeIns |
| hbppAmountChanged | GenerateWarrantyQuote, UpdatePackageWarranty |
| hbppSelectedChanged | GenerateWarrantyQuote, UpdatePackageWarranty |
| tradeSalePriceChanged | UpdatePackageTradeIns |

Full coverage. No field is missed. Correct decomposition.

---

## 10. Legacy Comparison — ProjectCost Price Comparison Strategy Changed (ACCEPTED)

Legacy matched project costs by ID (`existingProjectCost.Id == updatedProjectCost.Id && existingProjectCost.SalePrice != updatedProjectCost.SalePrice`). New uses order-agnostic sorted `SequenceEqual` on prices.

The new approach is simpler. It loses the ability to detect a project cost being replaced with another at the same price (unlikely edge case). The old approach couldn't detect new/removed project costs that happened to have the same count — which the new approach also can't detect. Both have the same practical sensitivity. Accepted.

---

## 11. Home-Type Removal Matrix — Verified Against Legacy (ACCEPTED)

The `RemoveInvalidProjectCosts` switch in `UpdatePackageHomeCommandHandler` was verified line-by-line against legacy `GetItemsToRemoveByHomeType` in `PackageDtoExtensions.cs`. Full match across all 3 home types (New, Used, Repo) and all category/item combinations.

| Cat/Item | New | Used | Repo | Status |
|---|---|---|---|---|
| 11/1 Cleaning | REMOVE | keep | REMOVE | MATCH |
| 11/2 Repair | REMOVE | keep | REMOVE | MATCH |
| 11/3 Parts | REMOVE | keep | REMOVE | MATCH |
| 11/4 Drapes | REMOVE | keep | keep | MATCH |
| 12/* Repo | REMOVE | REMOVE | keep | MATCH |
| 13/98 Tax Undercollection | REMOVE | REMOVE | REMOVE | MATCH |
| 15/4 Drapes | keep | REMOVE | REMOVE | MATCH |

---

## Action Items

| # | Priority | Finding | Effort |
|---|----------|---------|--------|
| ~~4~~ | ~~Low~~ | ~~Add `UseTaxCategoryNumber`/`UseTaxItemNumber` constants to 2 warranty handlers~~ | FIXED |
| ~~5~~ | ~~Low~~ | ~~Extract Use Tax constants to shared domain location, remove 10 `private const` declarations~~ | FIXED |
| 6 | Info | Verify W&A description string change is intentional with product/BFF team | — |
