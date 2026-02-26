# Land Handler: Legacy vs New — Comparison

## Overview

This document compares how the legacy Sales API (`rtl-domain-sales-api`) and the new modular monolith (`Modular-Template`) handle land line changes on a package. The legacy system used a single god-object `UpsertPackageCommandHandler` that processed the entire package in one call; the new system has a dedicated `UpdatePackageLandCommandHandler` endpoint.

---

## Architecture Comparison

| Aspect | Legacy | New |
|--------|--------|----|
| **Entry point** | `POST /sales/v1/{saleId}/packages` (full package upsert) | `PUT /api/v1/packages/{publicPackageId}/land` (land only) |
| **Handler** | `UpsertPackageCommandHandler` (god-object, ~900 lines) | `UpdatePackageLandCommandHandler` (~200 lines) |
| **Storage** | DynamoDB document (entire package as one JSON blob) | PostgreSQL (packages table + package_lines table + JSONB details) |
| **Concurrency** | Last-write-wins (DynamoDB) | EF Core concurrency token (TBD — `Package.Version`) |
| **Validation** | `LandValidator` (custom, pre-handler) | `UpdatePackageLandCommandValidator` (FluentValidation, pipeline behavior) |
| **Domain events** | None (fully synchronous, no event infrastructure) | `LandLineUpdatedDomainEvent` raised by `Package.AddLine` and `Package.RemoveLine<LandLine>()` |
| **Land type enums** | String constants (`"CustomerLandPayoff"`, etc.) | C# enums (`LandPurchaseType`, `CustomerLandType`, `LandInclusion`, `TypeOfLandWanted`) |

---

## Processing Steps — Side-by-Side

### Legacy (Steps 3A2, 3A6, 3A7, 3A9 of UpsertPackageCommandHandler)

```
POST /sales/v1/{saleId}/packages (body: full PackageDto with all lines)
  |
  +-- DefaultPackageValidator -> LandValidator
  +-- UpsertPackageCommandHandler.HandleAsync()
       |-- 3A1: HandleHomeTypeChange (if home type changed)
       |-- 3A2: HandleLandPackageLine        <-- LAND PRICING
       |-- 3A3: HandleWheelAndAxleChange     (not land-related)
       |-- 3A4: HandleTradeOverAllowance     (not land-related)
       |-- 3A5: HandleSellerPaidClosingCosts (not land-related)
       |-- 3A6: HandleLandPayoffProjectCost  <-- LAND PAYOFF SYNC
       |-- 3A7: HandleTaxErrors              <-- CLEAR TAX ERRORS
       |-- 3A8: HandleHomeFirstInsurance     (not land-related)
       +-- 3A9: HandleTaxCalculation         <-- TAX CHANGE DETECTION (ALWAYS LAST)
```

### New (Steps 1-7 of UpdatePackageLandCommandHandler)

```
PUT /api/v1/packages/{publicPackageId}/land (body: land fields only)
  |
  +-- FluentValidation pipeline -> UpdatePackageLandCommandValidator
  +-- UpdatePackageLandCommandHandler.Handle()
       |-- Step 1: Load package
       |-- Step 2: Snapshot land sale price
       |-- Step 3: Upsert land line (delete-then-insert)
       |-- Step 4: RecalculateLandPricing    <-- LAND PRICING
       |-- Step 5: SyncLandPayoffProjectCost <-- LAND PAYOFF SYNC
       |-- Step 6: FlagTaxRecalculationIfNeeded <-- TAX CHANGE DETECTION
       +-- Step 7: Finalize (events, GP, save)
  |
  +-- LandLineUpdatedDomainEvent -> LandLineUpdatedDomainEventHandler
       +-- Clears tax errors on the TaxLine  <-- CLEAR TAX ERRORS (via event)
```

---

## Land Pricing Matrix (Identical)

Both systems use the same 4-branch matrix to compute `SalePrice` and `EstimatedCost`:

| Land Type | SalePrice | EstimatedCost |
|-----------|-----------|---------------|
| `CustomerLandPayoff` | `PayoffAmountFinancing` | `PayoffAmountFinancing` |
| `LandPurchase` | `PurchasePrice` | `PurchasePrice` |
| `HomeCenterOwnedLand` | `LandSalesPrice` | `LandCost` |
| All others (`CustomerLandInLieu`, `HomeOnly`, `PrivateProperty`, `CommunityOrNeighborhood`) | 0 | 0 |

**Legacy code:**
```csharp
// HandleLandPackageLine() in UpsertPackageCommandHandler
landPackageLine.SalePrice = 0;
landPackageLine.EstimatedCost = 0;

if (details.LandInclusion == "CustomerLandPayoff") {
    landPackageLine.SalePrice = details.PayoffAmountFinancing ?? 0;
    landPackageLine.EstimatedCost = details.PayoffAmountFinancing ?? 0;
}
else if (details.TypeOfLandWanted == "LandPurchase") { ... }
else if (details.TypeOfLandWanted == "HomeCenterOwnedLand") { ... }
```

**New code:**
```csharp
// RecalculateLandPricing() in UpdatePackageLandCommandHandler
if (details.LandInclusion == LandInclusion.CustomerLandPayoff) {
    salePrice = details.PayoffAmountFinancing ?? 0m;
    estimatedCost = details.PayoffAmountFinancing ?? 0m;
}
else if (details.TypeOfLandWanted == TypeOfLandWanted.LandPurchase) { ... }
else if (details.TypeOfLandWanted == TypeOfLandWanted.HomeCenterOwnedLand) { ... }
```

**Verdict: Functionally identical.** The new code uses enums instead of string constants.

---

## Land Payoff Project Cost (Cat 2 / Item 1) — Identical

Both systems maintain a "shadow" project cost that mirrors the land line's pricing.

| Aspect | Legacy | New |
|--------|--------|----|
| **Category / Item** | `landPayoffCategoryId = 2`, `landPayoffItemId = 1` | `ProjectCostCategories.LandPayoff`, `ProjectCostItems.LandPayoff` |
| **ShouldExcludeFromPricing** | `true` | `true` |
| **Responsibility** | `"Seller"` | `Responsibility.Seller` |
| **RetailSalePrice** | `= SalePrice` (to pass validation) | `= landLine.SalePrice` |
| **Creation condition** | `LandInclusion == CustomerLandPayoff` OR `TypeOfLandWanted == LandPurchase` OR `TypeOfLandWanted == HomeCenterOwnedLand` | Same three conditions + `landLine.SalePrice > 0` |
| **Removal** | Removed when land has no priced type | Removed first, then re-added if applicable (remove-then-add pattern) |
| **Id preservation** | Preserves existing Id on update (`package.GetLandPayoffProjectCost()?.Id ?? Guid.NewGuid()`) | New Id every time (delete-then-insert pattern, EF handles identity) |

**Minor difference:** The new system adds a `SalePrice > 0` guard before creating the project cost. The legacy system does not — it creates the project cost even if `PayoffAmountFinancing`, `PurchasePrice`, or `LandSalesPrice` are 0. In practice this rarely matters since validators enforce `> 0` for these fields, but the new system is slightly more defensive.

**Verdict: Functionally equivalent.** The remove-then-add pattern in the new system is cleaner than the legacy's update-or-add-or-remove branching.

---

## Tax Change Detection — Equivalent with Structural Differences

### Legacy (`HandleTaxCalculation`)

The legacy system compares the **pre-mutation DynamoDB document** against the **post-mutation DTO** across 9 dimensions:

```csharp
var landSalePriceChanged = existingPackage.GetLandPackageLine()?.SalePrice
                        != updatedPackage.GetLandPackageLine()?.SalePrice;
```

When any change is detected (home type, stock number, home sale price, **land sale price**, project cost count/prices, HBPP, trade-in):
1. `RemoveTaxCalculation()` — zeros Tax.SalePrice, clears Tax.Details.Taxes, clears Tax.Details.Errors
2. `RemoveUseTaxProjectCost()` — removes Cat 9 / Item 21
3. `SetMustRecalculateTaxes(true)`

**Early exit:** Skips entirely if no prior tax calculation exists (`Taxes` is null/empty).

### New (`FlagTaxRecalculationIfNeeded`)

The new system only compares the single tax-affecting field for land — `SalePrice`:

```csharp
if (oldSalePrice == newSalePrice) return;
```

When the price changed:
1. `taxLine?.ClearCalculations()` — equivalent to legacy's `RemoveTaxCalculation()`
2. `package.RemoveProjectCost(UseTax)` — removes Cat 9 / Item 21
3. `package.FlagForTaxRecalculation()` — sets `MustRecalculateTaxes = true`

**No early exit on missing taxes** — the new system always flags for recalculation when the price changes, even if no prior tax calculation exists.

**Verdict: Functionally equivalent for land.** The legacy's multi-dimensional check (9 fields) was because the god-object handler processed everything at once. The new per-line handler only needs to check what its line affects — the land sale price.

---

## Tax Error Clearing — Different Mechanism, Same Result

| Aspect | Legacy | New |
|--------|--------|----|
| **Where** | `HandleTaxErrors()` (Step 3A7) — runs unconditionally on every update | `LandLineUpdatedDomainEventHandler` — triggered by domain event |
| **What** | Sets `taxPackageLine.Details.Errors = null` | Calls `taxLine.ClearErrors()` |
| **When** | Every package update, regardless of what changed | Only when a land line is added or removed |

**Verdict: Same result.** The legacy clears tax errors on every update (overkill but harmless). The new system does it only when land actually changes (more precise), triggered by the `LandLineUpdatedDomainEvent` that fires from `Package.AddLine(LandLine)` and `Package.RemoveLine<LandLine>()`.

---

## Gross Profit — Different Mechanism, Same Formula

| Aspect | Legacy | New |
|--------|--------|----|
| **Formula** | `Home.SalePrice - (Home.EstimatedCost + sum(ProjectCost.EstimatedCost where !ShouldExcludeFromPricing))` | `sum(line.SalePrice - line.EstimatedCost) where !ShouldExcludeFromPricing` |
| **When computed** | Computed property on `PackageDto` — re-derives on every read | `Package.RecalculateGrossProfit()` called explicitly before save |
| **Land line included?** | No — Land is type `"Land"`, not `"ProjectCost"`, so it's excluded from `GetProjectCosts()` | No — `LandLine.ShouldExcludeFromPricing = false`, but it IS included in the sum. Wait... |

**Important observation:** In the legacy system, the gross profit formula only sums Home + non-excluded ProjectCosts. The Land line (`ShouldExcludeFromPricing = false`) is NOT a ProjectCost, so it's excluded by type filtering. In the new system, `RecalculateGrossProfit()` sums ALL lines where `!ShouldExcludeFromPricing`. Since `LandLine.ShouldExcludeFromPricing = false`, the land line IS included in the new system's GP.

**This is a behavioral difference:**
- Legacy GP = `Home.SalePrice - Home.EstimatedCost - sum(PC.EstimatedCost)` (land excluded)
- New GP = `sum(all non-excluded lines: SalePrice - EstimatedCost)` (land included)

For most land types (CustomerLandInLieu, HomeOnly, PrivateProperty, CommunityOrNeighborhood), both `SalePrice` and `EstimatedCost` are 0 after repricing, so the difference is zero. But for the three priced types:

| Land Type | Legacy GP Impact | New GP Impact |
|-----------|-----------------|---------------|
| CustomerLandPayoff | 0 (not in GP) | `PayoffAmountFinancing - PayoffAmountFinancing = 0` |
| LandPurchase | 0 (not in GP) | `PurchasePrice - PurchasePrice = 0` |
| HomeCenterOwnedLand | 0 (not in GP) | `LandSalesPrice - LandCost` (COULD BE NON-ZERO) |

**For `HomeCenterOwnedLand`**, if `LandSalesPrice != LandCost` (dealer margin on land), the new system would include that margin in gross profit while the legacy system would not.

**Impact assessment:** This needs investigation — does the business intend for land dealer margin to be included in package GP? The Land Payoff shadow project cost is excluded (`ShouldExcludeFromPricing = true`), but the LandLine itself is not.

---

## Validation Comparison

| Rule | Legacy (`LandValidator`) | New (`UpdatePackageLandCommandValidator`) |
|------|--------------------------|-------------------------------------------|
| Max 1 land per package | `DefaultPackageValidator` checks `Count > 1` | Enforced by `RemoveLine<LandLine>()` (upsert semantics) |
| `LandPurchaseType` valid | Not explicitly validated (trusted from polymorphic JSON deserialization) | `Must(t => ValidLandPurchaseTypes.Contains(t))` |
| `CustomerLandType` required when `CustomerHasLand` | Yes | Yes |
| `LandInclusion` required when `CustomerOwnedLand` | Yes | Yes |
| `CustomerLandPayoff` — 6 fields | All validated: `EstimatedValue > 0`, `SizeInAcres > 0`, `PayoffAmountFinancing > 0`, `LandEquity` required, `OriginalPurchaseDate` required, `OriginalPurchasePrice > 0` | Same rules |
| `PrivateProperty` — phone number | Must be exactly 10 chars (no digit check) | Must match `^\d{10}$` (10 digits, stricter) |
| `CommunityOrNeighborhood` — community fields | 4 required: Name, ManagerName, ManagerPhone, ManagerEmail. `CommunityMonthlyCost` optional 0-1M | Same + `CommunityMonthlyCost` is **required** (not just optional) |
| `HomeCenterOwnedLand` — cross-validation | `EstimatedCost == LandCost`, `SalePrice == LandSalesPrice` | Same cross-validations |
| `LandPurchase` — PurchasePrice | Required, > 0, <= 1,000,000 | Same |
| Blank `TypeOfLandWanted` accepted? | Yes (silently — latent bug in legacy) | No — `NotEmpty()` when `CustomerWantsToPurchaseLand` |

**Improvements in new system:**
1. Phone number validation uses regex (`\d{10}`) instead of length check — catches non-digit characters
2. `TypeOfLandWanted` is required when applicable — closes a legacy bug where blank values were silently accepted
3. `CommunityMonthlyCost` is required for `CommunityOrNeighborhood` — legacy made it optional

---

## Domain Events

| Event | Legacy | New |
|-------|--------|----|
| `LandLineUpdatedDomainEvent` | N/A (no event infrastructure) | Raised by `Package.AddLine(LandLine)` and `Package.RemoveLine<LandLine>()` |
| `SaleSummaryChangedDomainEvent` | N/A | Raised by `package.Sale.RaiseSaleSummaryChanged()` in Step 7 |

The `LandLineUpdatedDomainEventHandler` clears tax errors on the TaxLine — this replaces the legacy's unconditional `HandleTaxErrors()` (Step 3A7).

---

## Commission — `LandPayoff` Field (Naming Trap)

Both systems send a `LandPayoff` value to iSeries for commission calculation. **This is NOT the Land Payoff project cost (Cat 2, Item 1).** It is the sum of **Category 3 (Land Improvements/Add-On)** project cost estimated costs.

```csharp
// Legacy — CommissionService.cs
LandPayoff = package.GetEstimatedLandImprovementCost()  // Sum of CategoryId==3 project costs

// New — CalculateCommissionCommandHandler.cs
LandPayoff = projectCosts
    .Where(pc => !pc.ShouldExcludeFromPricing && pc.Details?.CategoryId == 3)
    .Sum(pc => pc.EstimatedCost)
```

**Verdict: Identical logic, confusing naming preserved.**

---

## Fields Comparison

All 24 land detail fields are present in both systems:

| Field | Legacy Type | New Type | Notes |
|-------|-------------|----------|-------|
| `LandPurchaseType` | `string` | `LandPurchaseType` (enum) | |
| `CustomerLandType` | `string?` | `CustomerLandType?` (enum) | |
| `LandInclusion` | `string?` | `LandInclusion?` (enum) | |
| `TypeOfLandWanted` | `string?` | `TypeOfLandWanted?` (enum) | |
| `EstimatedValue` | `decimal?` | `decimal?` | |
| `SizeInAcres` | `decimal?` | `decimal?` | |
| `PropertyOwner` | `string?` | `string?` | |
| `PropertyOwnerPhoneNumber` | `string?` | `string?` | |
| `PropertyLotRent` | `decimal?` | `decimal?` | |
| `PayoffAmountFinancing` | `decimal?` | `decimal?` | |
| `LandEquity` | `decimal?` | `decimal?` | |
| `OriginalPurchaseDate` | `DateTimeOffset?` | `DateTimeOffset?` | New converts `DateTime? -> DateTimeOffset` in handler |
| `OriginalPurchasePrice` | `decimal?` | `decimal?` | |
| `FinancedBy` | `string?` | `string?` | |
| `PurchasePrice` | `decimal?` | `decimal?` | |
| `Realtor` | `string?` | `string?` | |
| `LandStockNumber` | `string?` | `string?` | |
| `LandCost` | `decimal?` | `decimal?` | |
| `LandSalesPrice` | `decimal?` | `decimal?` | |
| `CommunityNumber` | `int?` | `int?` | |
| `CommunityName` | `string?` | `string?` | |
| `CommunityManagerName` | `string?` | `string?` | |
| `CommunityManagerPhoneNumber` | `string?` | `string?` | |
| `CommunityManagerEmail` | `string?` | `string?` | |
| `CommunityMonthlyCost` | `decimal?` | `decimal?` | |

---

## Resolved Items

### 1. GP included land margin for HomeCenterOwnedLand — FIXED

`LandLine.ShouldExcludeFromPricing` was `false`, causing `RecalculateGrossProfit()` to include land in GP. The legacy system never included land in GP. **Fixed:** changed to `true`. Land's financial impact flows through the shadow Land Payoff project cost (Cat 2, Item 1) which is also excluded from GP — this is bookkeeping-only data for commission/funding.

### 2. `LandParcelId` inventory link not resolved — FIXED

The handler was not resolving `LandParcelId` for `HomeCenterOwnedLand` scenarios, leaving the FK always null and making the appraisal-change and inventory-removal downstream handlers inert. **Fixed:** added `FindLandParcelByHomeCenterAndStockAsync` to `IInventoryCacheQueries` and wired it into `UpsertLandLine` — same pattern as the home handler's `onLotHomeId` resolution. Returns `Error.NotFound` if the land parcel isn't in the cache.

### 3. Legacy `IsLandDOT` flag — NO ACTION NEEDED

Already fully implemented in the new system as `CdcProjectCostCategory.IsLandDot` (category-level, which is more correct than the legacy item-level flag). Present in: CDC entity, EF config, DB migration, query handler, endpoint response, and snapshotted into `ProjectCostDetails.CategoryIsLandDot`.
