# Sales Module — Pricing & Calculation Reference

> Central reference for every formula, pricing matrix, and auto-generated line in the
> Sales module. Covers both local domain logic and delegated iSeries calculations.

---

## Quick Reference — Key Pricing Terms

### Per-Line Fields (on every `PackageLine`)

| Term | What It Means |
|------|---------------|
| **SalePrice** | The negotiated price charged to the buyer for this line. For a Home, it's what the customer pays. For a Credit (down payment / concession), it's the dollar amount of the credit. Encrypted at rest. |
| **EstimatedCost** | The dealer's cost basis — what the dealership paid for this item (invoice cost, wholesale cost, subcontractor cost). For Credits it's always `0`. Encrypted at rest. |
| **RetailSalePrice** | The maximum allowed (ceiling) sale price — derived from cost x iSeries pricing multiplier. Think of it as the "sticker price" or MSRP. `SalePrice` must never exceed this. Encrypted at rest. |
| **ShouldExcludeFromPricing** | Flag controlling whether this line participates in the GP formula. When `true`, the line's `SalePrice - EstimatedCost` is excluded from Gross Profit. |
| **Responsibility** | Who bears the cost: `Buyer` (customer pays) or `Seller` (dealer absorbs). Nullable. |

### Package-Level Fields

| Term | What It Means |
|------|---------------|
| **Gross Profit (GP)** | `SUM(SalePrice - EstimatedCost)` for all non-excluded lines. The dealer's total margin on the package. Recalculated locally before every save. Encrypted at rest. |
| **Commissionable Gross Profit (CGP)** | The subset of GP on which sales commissions are earned. Computed entirely by iSeries — never calculated locally. Accounts for trade-in adjustments, land payoff, and other deductions that reduce the commissionable base. Encrypted at rest. |
| **MustRecalculateTaxes** | Dirty flag — `true` means tax results are stale and the package needs to go through iSeries tax calculation before submission. Set on any pricing-relevant change, cleared after successful tax calc. |

### Home Cost Fields (on `HomeDetails`)

| Term | What It Means |
|------|---------------|
| **BaseCost** | Factory cost of the home before options. |
| **OptionsCost** | Cost of manufacturer-installed options (flooring, appliances, etc.). |
| **FreightCost** | Shipping cost from factory to dealer lot. |
| **InvoiceCost** | Manufacturer invoice amount (typically Base + Options + Freight). |
| **NetInvoice** | Invoice amount after manufacturer rebates/adjustments — what the dealer actually remits. |
| **GrossCost** | Total gross cost before any rebates are netted out. |
| **RebateOnMfgInvoice** | Manufacturer rebate credited back to the dealer (volume/program rebates). Reduces effective cost. |
| **TaxIncludedOnInvoice** | Sales/excise tax the manufacturer included on their invoice to the dealer (distinct from customer-facing tax). |
| **CarrierFrameDeposit** | Refundable deposit paid to the carrier for the transport frame/chassis ("running gear"). |

### Trade-In Fields (on `TradeInDetails`)

| Term | What It Means |
|------|---------------|
| **TradeAllowance** | What the dealer offers the customer in credit for their trade-in — the "buy price." |
| **BookInAmount** | Wholesale/book value of the trade-in — what the dealer can realistically recover on resale. |
| **PayoffAmount** | Outstanding loan balance the customer owes on the traded-in item. Dealer must pay this off. |
| **Trade Over Allowance** | `TradeAllowance - BookInAmount` — the dealer's net cost for honoring the trade deal above book value. Auto-generates a project cost (Cat 10 / Item 9) that reduces GP. |

### Land Fields (on `LandDetails`)

| Term | What It Means |
|------|---------------|
| **PurchasePrice** | Third-party land purchase price (when `TypeOfLandWanted = LandPurchase`). Drives both SalePrice and EstimatedCost equally. |
| **LandSalesPrice** | Customer-facing price for home-center-owned land (`HomeCenterOwnedLand`). Drives LandLine.SalePrice. |
| **LandCost** | Dealer's cost basis for home-center-owned land. Drives LandLine.EstimatedCost. The gap `LandSalesPrice - LandCost` is the dealer's land margin. |
| **PayoffAmountFinancing** | Outstanding mortgage on customer-owned land (`CustomerLandPayoff`). Drives both SalePrice and EstimatedCost equally. |
| **EstimatedValue** | Appraised market value of customer's land. Informational — does not drive pricing directly. |
| **LandEquity** | Customer's equity in their land (`EstimatedValue - PayoffAmountFinancing`). Can be negative. Informational. |

### Tax Fields (on `TaxDetails`)

| Term | What It Means |
|------|---------------|
| **TaxLine.SalePrice** | Total tax amount collected from the buyer (sum of all `TaxItem.ChargedAmount` values). |
| **PreviouslyTitled** | Whether the home was previously titled/registered — affects tax treatment in some states. |
| **TaxExemptionId** | iSeries ID for an applicable tax exemption. Non-null and non-zero = tax exempt. |

### Credit Types (on `CreditDetails`)

| Term | What It Means |
|------|---------------|
| **DownPayment** | Cash paid by the buyer up front. `Responsibility = Buyer`. No side effects. |
| **Concessions** | Seller-offered price reduction. `Responsibility = Seller`. Auto-generates a Seller Paid Closing Cost project cost (Cat 14 / Item 1) that reduces GP. |

---

## Formulas At a Glance

Every piece of math we compute locally in code. iSeries-delegated calculations
(tax amounts, commission, W&A pricing, insurance/warranty premiums) are not listed
here — those are pass-through values we store but don't compute.

### Core Pricing

```
Gross Profit = SUM( line.SalePrice - line.EstimatedCost )
               WHERE line.ShouldExcludeFromPricing == false
```
> `Package.RecalculateGrossProfit()` — called once before every SaveChangesAsync.

```
Commissionable Gross Profit = <iSeries-computed, stored verbatim>
```
> Never computed locally. Set from `iSeriesAdapter.CalculateCommission()` result.

### Rounding

```
All monetary values = Math.Round(value, 2)
```
> Applied in every `PackageLine.Create()` factory AND in `PackageLine.UpdatePricing()`.
> No unrounded monetary value can enter the domain.

### Land Pricing — 4-Branch Matrix

```
IF   LandInclusion == CustomerLandPayoff:
       SalePrice = PayoffAmountFinancing ?? 0
       EstimatedCost = PayoffAmountFinancing ?? 0

ELIF TypeOfLandWanted == LandPurchase:
       SalePrice = PurchasePrice ?? 0
       EstimatedCost = PurchasePrice ?? 0

ELIF TypeOfLandWanted == HomeCenterOwnedLand:
       SalePrice = LandSalesPrice ?? 0
       EstimatedCost = LandCost ?? 0          ← dealer margin (SP ≠ EC)

ELSE:
       SalePrice = 0
       EstimatedCost = 0
```
> `RecalculateLandPricing()` in UpdatePackageLandCommandHandler. RetailSalePrice
> is always preserved from the client request — never overwritten.

### Trade Over Allowance

```
per trade-in:
  overAllowance = TradeAllowance - BookInAmount
  IF overAllowance > 0:
    → create ProjectCost(Cat 10, Item 9): SalePrice = 0, EstimatedCost = overAllowance
```
> `SyncTradeOverAllowance()` in UpdatePackageTradeInsCommandHandler.
> Reduces GP by the over-allowance amount (cost with no offsetting revenue).

### Total Tax

```
TaxLine.SalePrice = StateTax + CityTax + CountyTax
                  + GrossReceiptCityTax   (TN only, else 0)
                  + GrossReceiptCountyTax (TN only, else 0)
                  + MHIT                  (TX only, else 0)
```
> Individual tax amounts come from iSeries. The sum is local.

### Commission Request Fields

```
Cost = HomeLine.EstimatedCost
     + SUM( ProjectCostLine.EstimatedCost WHERE !ShouldExcludeFromPricing )

LandPayoff = SUM( ProjectCostLine.EstimatedCost
             WHERE CategoryId == 3 (Land) AND !ShouldExcludeFromPricing )
```
> Sent to iSeries as inputs to the commission calculation.

### UpdateAllowances Aggregates

```
TradeAllowance    = SUM( tradeIn.Details.TradeAllowance )     across all trade-ins
BookInAmount      = SUM( tradeIn.Details.BookInAmount )       across all trade-ins
TotalAddOnCost    = SUM( PC.EstimatedCost )                   non-excluded PCs only
TotalAddOnSalePrice = SUM( PC.SalePrice )                     non-excluded PCs only
```
> Sent to iSeries as inputs to both tax and commission calculations.

### Auto-Generated Shadow Costs (mirrors, no arithmetic)

```
Land Payoff PC:           SalePrice = landLine.SalePrice
  (Cat 2, Item 1)         EstimatedCost = landLine.EstimatedCost
                           Only when SalePrice > 0 AND priced land type

Use Tax PC:               SalePrice = EstimatedCost = iSeries UseTax amount
  (Cat 9, Item 21)         Only when UseTax > 0

Seller Paid Closing Cost: SalePrice = 0, EstimatedCost = concessionAmount
  (Cat 14, Item 1)         Only when concessionAmount > 0

W&A PC:                   SalePrice = iSeries waResult.SalePrice
  (Cat 1, Item 28 or 29)   EstimatedCost = iSeries waResult.Cost
```

### Wire Encoding

```
DomicileCode = "{HomeCondition}{SectionCode}"
  HomeCondition: N=New, U=Used, R=Repo
  SectionCode:   S (floors ≤ 1), D (floors > 1)

iSeriesDateInt = Year × 10000 + Month × 100 + Day
  e.g. 2025-02-14 → 20250214
```

### Validation Only (not used downstream)

```
CommissionSplitPercentages: SUM(member.CommissionSplitPercentage) must == 100
```

---

## Table of Contents

1. [Gross Profit (GP)](#1-gross-profit-gp)
2. [Commissionable Gross Profit (CGP)](#2-commissionable-gross-profit-cgp)
3. [ShouldExcludeFromPricing Matrix](#3-shouldexcludefrompricing-matrix)
4. [GP Impact Per Line Type](#4-gp-impact-per-line-type)
5. [Land Pricing — 4-Branch Matrix](#5-land-pricing--4-branch-matrix)
6. [Land Payoff Shadow Project Cost](#6-land-payoff-shadow-project-cost)
7. [W&A (Wheels & Axles) Pricing](#7-wa-wheels--axles-pricing)
8. [Auto-Generated Project Cost Lines](#8-auto-generated-project-cost-lines)
9. [Tax Calculation — iSeries Flow](#9-tax-calculation--iseries-flow)
10. [Commission Calculation — iSeries Flow](#10-commission-calculation--iseries-flow)
11. [UpdateAllowances — Shared Prerequisite](#11-updateallowances--shared-prerequisite)
12. [Tax Change Detection Triggers](#12-tax-change-detection-triggers)
13. [Funding (AppId Resolution)](#13-funding-appid-resolution)
14. [Sale Summary Propagation](#14-sale-summary-propagation)
15. [iSeries Adapter Call Index](#15-iseries-adapter-call-index)
16. [Well-Known Project Cost IDs](#16-well-known-project-cost-ids)

---

## 1. Gross Profit (GP)

**File:** `Domain/Packages/Package.cs` — `RecalculateGrossProfit()`

```
GP = SUM( line.SalePrice - line.EstimatedCost )
     WHERE line.ShouldExcludeFromPricing == false
```

Every command handler calls `RecalculateGrossProfit()` once, immediately before
`SaveChangesAsync()`. This matches the legacy pattern where GP was a computed property
evaluated at persistence time, not after every individual line mutation.

**Key insight:** The formula is generic over all line types. Whether a line participates
depends entirely on its `ShouldExcludeFromPricing` flag (see [section 3](#3-shouldexcludefrompricing-matrix)).

---

## 2. Commissionable Gross Profit (CGP)

**File:** `Application/Commission/CalculateCommission/CalculateCommissionCommandHandler.cs`

CGP is **not computed locally** — it is computed entirely by the iSeries system and stored
verbatim:

```
iSeries.CalculateCommission(...) → response.CommissionableGrossProfit
  → package.SetCommissionableGrossProfit(value)
```

CGP is initialized to `0m` in `Package.Create()` and stays at zero until
`CalculateCommission` is explicitly called. The local `RecalculateGrossProfit()` method
never touches CGP.

---

## 3. ShouldExcludeFromPricing Matrix

Every `PackageLine` subclass declares whether it participates in the GP formula:

| Line Type | Value | How Set | Notes |
|-----------|-------|---------|-------|
| `HomeLine` | `false` | `override => false` | Always in GP |
| `LandLine` | `true` | `override => true` | Excluded — impact flows via Land Payoff PC |
| `TradeInLine` | `true` | `override => true` | Credit, not a price component |
| `CreditLine` | `true` | `override => true` | Down payment / concessions |
| `SalesTeamLine` | `true` | `override => true` | Metadata only, all-zero prices |
| `WarrantyLine` | configurable | constructor param | Always `false` in practice |
| `InsuranceLine` | configurable | constructor param | Always `false` in practice |
| `TaxLine` | configurable | constructor param | Always `false` in practice |
| `ProjectCostLine` | configurable | constructor param | Varies by category — see below |

**ProjectCostLine values for auto-generated costs:**

| Auto-Generated Cost | Cat/Item | Excluded? | Why |
|---------------------|----------|-----------|-----|
| Land Payoff | 2 / 1 | **Yes** | Mirrors land line; would double-count |
| W&A Rental | 1 / 28 | No | Real cost with margin |
| W&A Purchase | 1 / 29 | No | Real cost with margin |
| Use Tax | 9 / 21 | No | SalePrice == EstimatedCost, so net zero |
| Trade Over Allowance | 10 / 9 | No | Reduces GP (EstimatedCost > 0, SalePrice = 0) |
| Seller Paid Closing Cost | 14 / 1 | No | Reduces GP (EstimatedCost > 0, SalePrice = 0) |

---

## 4. GP Impact Per Line Type

Expanded view showing how each line type actually affects the GP number:

| Line Type | GP Contribution | Typical Values |
|-----------|----------------|----------------|
| `HomeLine` | `+SalePrice - EstimatedCost` | Main profit driver |
| `LandLine` | None (excluded) | Impact via Land Payoff PC instead |
| `WarrantyLine` | `+SalePrice - 0` | SalePrice = premium, Cost = 0 |
| `InsuranceLine` | `+SalePrice - 0` | SalePrice = premium, Cost = 0 |
| `TaxLine` | `+SalePrice - 0` | SalePrice = total tax, Cost = 0 |
| PC: Land Payoff | None (excluded) | — |
| PC: Use Tax | `+UseTax - UseTax` = **0 net** | Sale = Cost, washes out |
| PC: Trade Over Allowance | `+0 - overAllowance` = **negative** | Reduces GP |
| PC: Seller Paid Closing Cost | `+0 - concessionAmt` = **negative** | Reduces GP |
| PC: W&A | `+waSalePrice - waCost` | May have dealer margin |
| PC: User-managed | `+SalePrice - EstimatedCost` | Configurable |
| `TradeInLine` | None (excluded) | — |
| `CreditLine` | None (excluded) | — |
| `SalesTeamLine` | None (excluded) | — |

---

## 5. Land Pricing — 4-Branch Matrix

**File:** `Application/Packages/UpdatePackageLand/UpdatePackageLandCommandHandler.cs` —
`RecalculateLandPricing()`

The request's `SalePrice` / `EstimatedCost` are starting values that get **overwritten**
by the land type matrix. `RetailSalePrice` is always preserved from the client request.

| Condition | SalePrice Source | EstimatedCost Source | Notes |
|-----------|-----------------|---------------------|-------|
| `LandInclusion == CustomerLandPayoff` | `PayoffAmountFinancing` | `PayoffAmountFinancing` | Equal — pass-through payoff |
| `TypeOfLandWanted == LandPurchase` | `PurchasePrice` | `PurchasePrice` | Equal — no dealer margin |
| `TypeOfLandWanted == HomeCenterOwnedLand` | `LandSalesPrice` | `LandCost` | **Unequal** — dealer margin |
| Everything else | `0` | `0` | No pricing impact |

**Priority:** `CustomerLandPayoff` check runs first (it's on `LandInclusion`, not
`TypeOfLandWanted`). If that matches, the `TypeOfLandWanted` branches are skipped.

---

## 6. Land Payoff Shadow Project Cost

**File:** `Application/Packages/UpdatePackageLand/UpdatePackageLandCommandHandler.cs` —
`SyncLandPayoffProjectCost()`

A shadow `ProjectCostLine` (Cat 2 / Item 1) that **mirrors** the land line's pricing.
Excluded from GP (`ShouldExcludeFromPricing = true`) but consumed by commission and
funding calculations.

**Created when:** Land has a positive `SalePrice` AND is one of the three priced types
(`CustomerLandPayoff`, `LandPurchase`, `HomeCenterOwnedLand`).

```
SalePrice       = landLine.SalePrice
EstimatedCost   = landLine.EstimatedCost
RetailSalePrice = landLine.SalePrice
```

**Pattern:** Remove-then-add (same PUT semantics as the land line itself).

---

## 7. W&A (Wheels & Axles) Pricing

**File:** `Application/Packages/UpdatePackageHome/UpdatePackageHomeCommandHandler.cs` —
`UpsertWheelsAndAxlesProjectCost()`

Two iSeries paths, selected by home source type:

Both paths return a `WheelAndAxlePriceResult(decimal SalePrice, decimal Cost)` with two
independent fields. The code uses them separately — there is no structural guarantee
that the two values are equal for either path.

### Path 1 — Stock Number Lookup (OnLot / VmfHomes)

```
iSeries.GetWheelAndAxlePriceByStock(homeCenterNumber, stockNumber)
  → returns WheelAndAxlePriceResult { SalePrice, Cost }
```

### Path 2 — Count-Based (all other home types)

```
iSeries.CalculateWheelAndAxlePriceByCount(numberOfWheels, numberOfAxles)
  → returns WheelAndAxlePriceResult { SalePrice, Cost }
```

### W&A Option → Category/Item Mapping

| `WheelAndAxlesOption` | Category | Item |
|----------------------|----------|------|
| `Rent` | 1 | 28 (`WaRental`) |
| `Purchase` | 1 | 29 (`WaPurchase`) |

W&A project costs are removed when the home type changes (via `RemoveInvalidProjectCosts`).

---

## 8. Auto-Generated Project Cost Lines

These are system-managed `ProjectCostLine` records. Users cannot create or edit them
directly — they're synced by specific command handlers. The `UpdatePackageProjectCosts`
handler guards against user edits to these keys:

```csharp
AutoGeneratedKeys = {
    (9,  21),   // Use Tax
    (2,  1),    // Land Payoff
    (10, 9),    // Trade Over Allowance
    (14, 1),    // Seller Paid Closing Cost
    (1,  28),   // W&A Rental
    (1,  29)    // W&A Purchase
}
```

| Name | Cat/Item | Created By | Pricing Source | Excluded? |
|------|----------|------------|---------------|-----------|
| Land Payoff | 2 / 1 | `UpdatePackageLand` | Mirrors `LandLine` | Yes |
| Use Tax | 9 / 21 | `CalculateTaxes` | iSeries `UseTax` result | No |
| Trade Over Allowance | 10 / 9 | `UpdatePackageTradeIns` | `TradeAllowance - BookInAmount` per trade | No |
| Seller Paid Closing Cost | 14 / 1 | `UpdatePackageConcessions` | Concession credit amount | No |
| W&A Rental | 1 / 28 | `UpdatePackageHome` | iSeries W&A pricing | No |
| W&A Purchase | 1 / 29 | `UpdatePackageHome` | iSeries W&A pricing | No |

### Trade Over Allowance Formula

One project cost line is created **per trade-in** where the dealer allows more than
the trade is worth:

```
overAllowance = TradeAllowance - BookInAmount    (only when positive)
SalePrice     = 0
EstimatedCost = overAllowance
```

This reduces GP by the over-allowance amount (cost with no offsetting revenue).

---

## 9. Tax Calculation — iSeries Flow

**File:** `Application/Tax/CalculateTaxes/CalculateTaxesCommandHandler.cs`

A 13-step orchestration that delegates the actual tax math to iSeries:

```
Step 1:  Load package (full aggregate + sale context)
Step 2:  Guard: HomeLine, DeliveryAddress, RetailLocation, PreviouslyTitled required
Step 3:  Resolve AppId from FundingRequestCache
Step 4:  iSeries: DELETE prior tax question answers
Step 5:  iSeries (parallel): UpdateAllowances + InsertQuestionAnswers
Step 6:  iSeries: CalculateTax
Step 7:  Handle soft errors from iSeries Messages[]
Step 8:  State-specific post-processing:
           TN → GrossReceiptCityTax, GrossReceiptCountyTax
           TX → ManufacturedHomeInventoryTax (MHIT)
Step 9:  Sync Use Tax project cost (Cat 9 / Item 21)
Step 10: Build 6 TaxItems (always all 6 — nulled out per state)
Step 11: totalTaxSalePrice = StateTax + CityTax + CountyTax + GRCT + GRCO + MHIT
Step 12: ClearTaxRecalculationFlag()
Step 13: RaiseSaleSummaryChanged + RecalculateGrossProfit + SaveChanges
```

### DomicileCode Construction

Sent to iSeries as part of the tax request:

```
DomicileCode = "{HomeCondition}{SectionCode}"
  HomeCondition: N = New, U = Used, R = Repo
  SectionCode:   S = single-section (floors <= 1), D = double/multi-section

ModCode = ModularClassification mapped to:
  HUD     → 'N'
  OnFrame → 'F'
  OffFrame→ 'O'
```

### Tax Items Returned

| Tax Type | Always Present | State-Specific |
|----------|---------------|----------------|
| State Tax | Yes | — |
| City Tax | Yes | — |
| County Tax | Yes | — |
| Gross Receipt City Tax | No | TN only |
| Gross Receipt County Tax | No | TN only |
| Manufactured Home Inventory Tax (MHIT) | No | TX only |

---

## 10. Commission Calculation — iSeries Flow

**File:** `Application/Commission/CalculateCommission/CalculateCommissionCommandHandler.cs`

```
Step 1: Load package + sale context
Step 2: Validate: DeliveryAddress, HomeCenterZip, HomeLine,
        SalesTeamLine with members, AuthorizedUserIds
Step 3: Resolve EmployeeNumbers from authorized_users cache
Step 4: Resolve AppId from FundingRequestCache
Step 5: UpdateAllowances to iSeries (must precede commission calc)
Step 6: CalculateCommission to iSeries
Step 7: Store CGP: package.SetCommissionableGrossProfit(result.CGP)
```

### Commission Request — Key Fields

```
Cost = HomeLine.EstimatedCost
       + SUM(ProjectCostLine.EstimatedCost WHERE !ShouldExcludeFromPricing)

LandPayoff = SUM(ProjectCostLine.EstimatedCost
             WHERE CategoryId == 3 (Land) AND !ShouldExcludeFromPricing)

Splits[] = { EmployeeNumber, GrossPayPercentage = CommissionSplitPercentage }
```

**Note:** `LandImprovements` and `AdjustedCost` are always sent as `0m` — reserved for
future use.

---

## 11. UpdateAllowances — Shared Prerequisite

**Called by:** `CalculateTaxes` (Step 5) and `CalculateCommission` (Step 5)

**iSeries URL:** `POST v1/taxes/allowances`

Sends the full financial picture of the package to iSeries so it can compute the tax
basis or commission basis:

| Field | Source |
|-------|--------|
| `HomeSalePrice` | `homeLine.SalePrice` |
| `HomeNetInvoice` | `homeLine.Details.NetInvoice` |
| `FreightCost` | `homeLine.Details.FreightCost` |
| `CarrierFrameDeposit` | `homeLine.Details.CarrierFrameDeposit` |
| `GrossCost` | `homeLine.Details.GrossCost` |
| `TaxIncludedOnInvoice` | `homeLine.Details.TaxIncludedOnInvoice` |
| `RebateOnMfgInvoice` | `homeLine.Details.RebateOnMfgInvoice` |
| `TradeAllowance` | `SUM(tradeIns.TradeAllowance)` |
| `BookInAmount` | `SUM(tradeIns.BookInAmount)` |
| `TradeInType` | Mapped from first trade-in's `TradeType` (see below) |
| `PreviouslyTitled` | Tax path: `"No"` → `""`, else as-is. Commission path: raw stored value (no mapping). |
| `IsTaxExempt` | `taxExemptionId is not null and not 0` |
| `TotalAddOnCost` | `SUM(PC.EstimatedCost WHERE !excluded)` |
| `TotalAddOnSalePrice` | `SUM(PC.SalePrice WHERE !excluded)` |
| `AddOns[]` | `{ CategoryNumber, ItemNumber, Cost, SalePrice }` per non-excluded PC |

### TradeInType Code Mapping

```
"Single Wide"    → 'S'
"Double Wide"    → 'D'
"Modular Home"   → 'D'   (NOT 'M' — legacy-faithful)
"Motorcycle"     → 'C'   (NOT 'M' — legacy-faithful)
"Boat"           → 'B'
"Motor Vehicle"  → 'V'
"Travel Trailer" → 'T'
"5th Wheel"      → 'F'
"Fifth Wheel"    → 'F'
_ (fallback)     → first character of the string
```

---

## 12. Tax Change Detection Triggers

`MustRecalculateTaxes` is flagged `true` by these handlers when pricing-relevant data
changes. Each handler snapshots before/after state and only flags when a real change
occurred (prevents unnecessary flags on no-op saves):

| Handler | Trigger Condition |
|---------|-------------------|
| `UpdatePackageLand` | Land sale price changed |
| `UpdatePackageHome` | Home type, stock number, sale price, or project costs changed |
| `UpdatePackageWarranty` | Warranty amount or selection changed |
| `UpdatePackageTradeIns` | Trade-in prices or project cost prices changed |
| `UpdatePackageProjectCosts` | Project cost count or prices changed |
| `UpdatePackageConcessions` | Non-excluded line count changed |
| `UpdateDeliveryAddress` | Delivery state or location changed (location only for Draft packages) |
| `UpdatePackageTax` | Always flags (tax config itself changed) |
| `GenerateWarrantyQuote` | Warranty quote recalculated |

When flagged, the handler also:
1. Calls `taxLine.ClearCalculations()` — zeroes out cached tax amounts, preserves config
2. Removes the stale Use Tax project cost (Cat 9 / Item 21)

---

## 13. Funding (AppId Resolution)

The Sales module does **not** compute funding amounts. It maintains a
`FundingRequestCache` (populated via integration events from the Funding module) whose
primary purpose is to supply the `AppId` needed by iSeries for tax and commission calls.

**AppId extraction** (used in both tax and commission handlers):

```
FundingRequestCache.FundingKeys (JSONB array):
  [{"Key": "AppId", "Value": "999999"}, ...]

Parse → find element where Key == "AppId" → parse Value as int
```

`Package.Create()` raises `PackageReadyForFundingDomainEvent` with `RequestAmount = 0m`,
which triggers the Funding module to create its initial records.

---

## 14. Sale Summary Propagation

No local "sale total" is computed. Summary data is propagated to the Inventory module as
a `SaleSummaryChangedIntegrationEvent`:

```
StockNumber, SaleId, CustomerName, ReceivedInDate,
OriginalRetailPrice (= homeLine.RetailSalePrice),
CurrentRetailPrice  (= homeLine.SalePrice),
UpdatedAt
```

**Trigger:** `SaleSummaryChangedDomainEvent` is raised by:
- `Sale.Create()` — new sale
- `Sale.UpdateStatus()` — status change
- `Sale.RaiseSaleSummaryChanged()` — called explicitly by `UpdatePackageLand`,
  `UpdatePackageHome`, and `CalculateTaxes`

---

## 15. iSeries Adapter Call Index

| Method | URL | Purpose | Called From |
|--------|-----|---------|------------|
| `GetWheelAndAxlePriceByStock` | `GET v1/inventory/home-inventory-ancillary-data` | W&A price by stock | `UpdatePackageHome` |
| `CalculateWheelAndAxlePriceByCount` | `GET v1/wheels-and-axles/price` | W&A price by counts | `UpdatePackageHome` |
| `DeleteTaxQuestionAnswers` | `POST v1/taxes/questions/delete` | Clear prior Q&A | `CalculateTaxes` |
| `UpdateAllowances` | `POST v1/taxes/allowances` | Send financial data | `CalculateTaxes`, `CalculateCommission` |
| `InsertTaxQuestionAnswers` | `POST v1/taxes/questions/insert` | State-specific Q&A | `CalculateTaxes` |
| `CalculateTax` | `POST v1/taxes` | Compute all tax types | `CalculateTaxes` |
| `CalculateCommission` | `POST v1/commissions` | CGP + commission splits | `CalculateCommission` |
| `CalculateHomeFirstQuote` | `POST v1/insurance/home-first-quote` | Insurance premium | `GenerateHomeFirstQuote` |
| `CalculateWarrantyQuote` | `POST v1/insurance/hbpp-quote` | Warranty premium | `GenerateWarrantyQuote` |

---

## 16. Well-Known Project Cost IDs

### Categories (`ProjectCostCategories`)

| Name | ID |
|------|----|
| Wheels & Axles | 1 |
| Land Payoff | 2 |
| Land | 3 |
| Use Tax | 9 |
| Trade Over Allowance | 10 |
| Refurbishment | 11 |
| Repo Costs | 12 |
| Miscellaneous Tax | 13 |
| Seller Paid Closing Cost | 14 |
| Decorating | 15 |

### Items (`ProjectCostItems`)

| Name | ID | Used With Category |
|------|----|--------------------|
| W&A Rental | 28 | 1 |
| W&A Purchase | 29 | 1 |
| Land Payoff | 1 | 2 |
| Use Tax | 21 | 9 |
| Trade Over Allowance | 9 | 10 |
| Cleaning | 1 | 11 |
| Repair/Refurb | 2 | 11 |
| Refurb Parts | 3 | 11 |
| Drapes | 4 | 11 |
| Tax Undercollection | 98 | 13 |
| Seller Paid Closing Cost | 1 | 14 |
| Decorating Drapes | 4 | 15 |
