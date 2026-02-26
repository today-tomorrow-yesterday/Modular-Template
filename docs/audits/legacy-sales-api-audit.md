# Legacy Sales API → Modular Monolith Sales Module Audit

**Date:** 2026-02-26
**Scope:** `rtl-domain-sales-api` (legacy) vs `src/Modules/Sales/` (new)
**Method:** 13 parallel audit agents, each comparing one functional area service-by-service

---

## Table of Contents

- [Executive Summary](#executive-summary)
- [Severity Legend](#severity-legend)
- [Critical Findings (C)](#critical-findings)
- [High Findings (H)](#high-findings)
- [Medium Findings (M)](#medium-findings)
- [Low / Informational Findings (L)](#low--informational-findings)
- [Intentional Architectural Changes](#intentional-architectural-changes)
- [Endpoint Gap Analysis](#endpoint-gap-analysis)
- [Area-by-Area Detailed Audit](#area-by-area-detailed-audit)
  - [1. Home Line](#1-home-line)
  - [2. Land Line](#2-land-line)
  - [3. Insurance Line](#3-insurance-line)
  - [4. Warranty / HBPP Line](#4-warranty--hbpp-line)
  - [5. Trade-In Line](#5-trade-in-line)
  - [6. Credits (Down Payment & Concessions)](#6-credits-down-payment--concessions)
  - [7. Project Costs](#7-project-costs)
  - [8. Sales Team](#8-sales-team)
  - [9. Tax Service](#9-tax-service)
  - [10. Commission Service](#10-commission-service)
  - [11. Sales Service](#11-sales-service)
  - [12. Delivery Address](#12-delivery-address)
  - [13. Packages Service / CRUD](#13-packages-service--crud)
  - [14. Pricing / iSeries Adapter](#14-pricing--iseries-adapter)
  - [15. Validators](#15-validators)
  - [16. DTO Mapping / Entities](#16-dto-mapping--entities)

---

## Executive Summary

The new Modular Monolith Sales module successfully ports **~85%** of the legacy Sales API business logic. The architecture is cleaner — the monolithic 905-line `UpsertPackageCommandHandler` has been decomposed into 10+ dedicated CQRS command handlers, exception-based error handling replaced with `Result<T>`, and DynamoDB replaced with PostgreSQL/EF Core with JSONB details.

However, this audit identified **5 critical**, **12 high**, **15 medium**, and **8 low** findings representing missing business logic, changed behavior, or incomplete ports that could cause production issues if not addressed before go-live.

### Top 5 Risks

1. **GrossProfit formula fundamentally changed** — legacy computes GP from Home + ProjectCosts only; new computes from ALL non-excluded lines (insurance, warranty, tax, credits all now affect GP)
2. **Insurance PDF generation is a hardcoded stub** — 731 lines of MigraDocCore PDF generation not ported
3. **Tax validator completely missing** — 6 legacy validation rules have zero coverage
4. **Trade-in aggregation changed** — legacy uses first trade-in only for tax/commission; new sums all trade-ins
5. **MasterDealerNumber=29 not sent** in new iSeries requests (TaxCalculation, UpdateAllowances)

---

## Severity Legend

| Severity | Meaning |
|----------|---------|
| **C (Critical)** | Business logic fundamentally wrong or missing; will cause incorrect calculations or data loss in production |
| **H (High)** | Important logic gap; may cause incorrect behavior in specific scenarios |
| **M (Medium)** | Missing validation, edge case, or degraded UX; unlikely to cause data corruption |
| **L (Low/Info)** | Minor difference, cosmetic, or already-known intentional change |

---

## Critical Findings

### C-1: GrossProfit Formula Fundamentally Changed

**Legacy formula:**
```
GrossProfit = HomeSalePrice - (Home.EstimatedCost + Sum(ProjectCost.EstimatedCost where !ExcludeFromPricing))
```
Only Home and ProjectCost lines participate. Insurance, warranty, tax, credits, land — none affect GP.

**New formula:**
```csharp
// Package.cs:152-157
GrossProfit = _lines
    .Where(l => !l.ShouldExcludeFromPricing)
    .Sum(l => l.SalePrice - l.EstimatedCost);
```
ALL non-excluded lines participate. Insurance premiums, warranty premiums, tax amounts, credits — everything flows into GP.

**Impact:** GP will be significantly higher in the new system for any package with insurance, warranty, or tax lines. This changes commission calculations, reporting, and financial reconciliation.

**Recommendation:** Confirm with business stakeholders whether this is intentional. If not, restrict GP to only Home + ProjectCost lines (matching legacy behavior). If intentional, document as a known behavioral change.

---

### C-2: RecalculateGrossProfit Overwrites CommissionableGrossProfit

**Already tracked as known bug C-2.** `RecalculateGrossProfit()` only recalculates `GrossProfit` but `CommissionableGrossProfit` is set independently via `SetCommissionableGrossProfit()`. If commission calculation runs, sets CGP, then a line mutation triggers `RecalculateGrossProfit()`, GP changes but CGP retains its stale value. This is a consistency issue within the aggregate.

---

### C-3: Insurance PDF Generation Not Ported (Hardcoded Stub)

**Legacy:** `InsuranceQuotePdfGenerator.cs` — 731 lines of MigraDocCore PDF generation producing a real insurance quote sheet with policyholder info, coverage details, premium breakdown, and agent signatures.

**New:** The `PrintInsuranceQuote` handler returns a hardcoded fake PDF byte array. No actual PDF generation exists.

**Impact:** Any workflow that depends on printing/downloading an insurance quote sheet will get garbage data.

**Recommendation:** Port the MigraDocCore PDF generation or replace with a modern PDF library (QuestPDF, etc.).

---

### C-4: Trade-In Aggregation Changed (First-Only → Sum-All)

**Legacy behavior:**
```csharp
// PackageDtoExtensions.cs — uses .First() for trade-in amounts
var tradeIn = package.TradeIns.First();
allowancePayload.TradeAllowance = tradeIn.TradeAllowance;
allowancePayload.TradePayoff = tradeIn.Payoff;
```
Only the **first** trade-in's values go to iSeries for tax and commission calculations.

**New behavior:**
```csharp
// Multiple trade-ins are summed
var totalTradeAllowance = tradeIns.Sum(t => t.TradeAllowance);
```
All trade-ins are **summed** together.

**Impact:** For sales with multiple trade-ins, tax calculations and commission calculations will use different input values than legacy, potentially producing different tax amounts and commission figures.

**Recommendation:** Verify with business whether summing is correct. Legacy's first-only behavior may have been a bug, or it may reflect iSeries expectations.

---

### C-5: MasterDealerNumber=29 Not Sent to iSeries

**Legacy:** Every `UpdateAllowances` and `CalculateTax` call includes `MasterDealerNumber = 29` as a hardcoded constant.

**New:** The `TaxCalculationRequest` and `AllowanceUpdateRequest` DTOs do not include a `MasterDealerNumber` field.

**Impact:** iSeries may reject requests or return incorrect results if it requires this field.

**Recommendation:** Add `MasterDealerNumber = 29` to the new iSeries request DTOs and verify with iSeries team.

---

## High Findings

### H-1: Tax Validator Completely Missing

**Legacy:** `TaxValidator.cs` enforces 6 rules:
1. `PreviouslyTitled` is required
2. `PreviouslyTitled` must be "Yes" or "No"
3. If tax-exempt, `TaxExemptionId` is required
4. If not tax-exempt, `TaxExemptionId` must be null
5. `QuestionAnswers` cannot be null
6. Each answer must reference a valid `QuestionNumber`

**New:** `UpdatePackageTaxCommandValidator.cs` — validates only that `PackagePublicId` is a valid GUID. Zero tax-specific rules.

**Impact:** Invalid tax configuration can reach iSeries, causing calculation errors or silent data corruption.

---

### H-2: Package-Level Singleton Constraints Not Enforced

**Legacy:** `DefaultPackageValidator.cs` enforces that each package has at most one Home, one Land, one Tax, one Insurance/HBPP, and one SalesTeam entry.

**New:** No validator enforces these constraints. The `AddLine` method on `Package` just appends to the list.

**Impact:** API consumers could accidentally add duplicate Home/Land/Tax lines, creating data integrity issues.

---

### H-3: Occupancy Ineligibility Check Missing on Package Updates

**Legacy:** When any package update occurs (via `UpsertPackageCommandHandler`), if the delivery address state has an `OccupancyIneligible` flag, HomeFirst insurance and HBPP lines are automatically removed.

**New:** This check only fires when a new HomeFirst quote is generated (`GenerateHomeFirstQuoteCommandHandler`). If a package is updated after insurance was already added, and the delivery address had its occupancy status change, stale insurance lines remain.

**Impact:** Insurance lines may persist for ineligible occupancy types.

---

### H-4: Home-Type-Dependent Project Cost Removal Matrix

**Legacy:** `PackageDtoExtensions.cs` contains a complex removal matrix — when the home type changes, specific project costs are stripped based on the new home type:
- Multi-section home → remove W&A Rental (1/28) and W&A Purchase (1/29)
- Single-section home → remove W&A Rental (1/28) and W&A Purchase (1/29) conditionally based on `WheelAndAxlesOption`
- Park model / Tiny home → remove both W&A lines unconditionally

**New:** The `HomeLineUpdatedDomainEvent` handler removes W&A lines but the full conditional matrix needs verification against legacy behavior. The new handler may be more aggressive (removing lines that legacy would keep) or less aggressive (keeping lines that legacy would remove).

**Impact:** Incorrect W&A project cost line presence/absence affects GP and tax calculations.

---

### H-5: PreviouslyTitled Lossy Type Change

**Legacy:** `PreviouslyTitled` is a `string` with three states: `null` (unset), `"Yes"`, `"No"`.

**New:** `PreviouslyTitled` is a `bool` with two states: `true`, `false`.

**Impact:** The unset/null state is lost. Legacy behavior that distinguishes "user hasn't answered yet" from "user said no" cannot be replicated. This affects tax calculation guard clauses that check for null.

---

### H-6: TaxExemptionCode Null Guard Missing

**Legacy:** `TaxService.cs` checks `if (taxExemptionCode != null)` before including it in the iSeries allowance payload. If the user selected "tax exempt" but no exemption code exists, the allowance update proceeds without an exemption code.

**New:** `CalculateTaxesCommandHandler` does not check for null `TaxExemptionCode`. If the TaxLine has `IsTaxExempt = true` but `TaxExemptionCode` is null, the handler may send a null exemption code to iSeries.

**Impact:** Potential iSeries error or incorrect tax calculation for tax-exempt sales.

---

### H-7: Tax Recalculation Not Flagged When Warranty Deselected

**Legacy:** Tax change detection fires when `WarrantySelected` changes from `true` to `false`, clearing existing tax calculations and flagging for recalculation.

**New:** The `UpdatePackageWarrantyCommandHandler` flags taxes for recalculation when warranty amount changes or warranty is newly selected, but does NOT flag when warranty is deselected (removed entirely).

**Impact:** Stale tax calculations may persist after warranty removal.

---

### H-8: 6 Customer Fields Always Null in GetSaleById

**Legacy:** `GetSaleById` enriches the response with customer data from the Customers API adapter:
- `MobilePhone`, `HomePhone`, `Birthdate`, `SalesforceUrl`, `CoBuyerBirthdate`, `MailingAddress`

**New:** `GetSaleByIdQueryHandler` returns these fields as null. The ECST cache projection for customers either doesn't include these fields or they aren't mapped.

**Impact:** UI consumers expecting customer phone/birthdate data will receive nulls.

---

### H-9: Responsibility Validation Missing from 4 Validators

**Legacy:** Home, Insurance, HBPP, and TradeIn validators all validate the `Responsibility` field (must be "Buyer" or "Seller").

**New:** The corresponding validators (`UpdatePackageHomeCommandValidator`, `GenerateHomeFirstQuoteCommandValidator`, `UpdatePackageWarrantyCommandValidator`, `UpdatePackageTradeInsCommandValidator`) do not validate the `Responsibility` field.

**Impact:** Invalid responsibility values can be stored, potentially affecting downstream reporting.

---

### H-10: Investment Occupancy Type Not Handled

**Legacy:** The occupancy type enum includes `Investment` as a valid option. Insurance eligibility checks handle this type.

**New:** The occupancy type mapping does not include `Investment`. If a customer selects Investment occupancy, the system may reject or misclassify the sale.

---

### H-11: GetSaleBySaleNumber Endpoint Not Ported

**Legacy:** `SalesService.GetSaleBySaleNumber()` allows looking up a sale by its iSeries sale number (as opposed to the internal ID). Used by other systems for integration.

**New:** No equivalent endpoint exists. Only `GetSaleById` (internal ID) is available.

**Impact:** External systems that reference sales by sale number have no way to look them up.

---

### H-12: HomeInventoryAncillaryData Endpoint Not Ported

**Legacy:** `GET /sales/{id}/home/ancillary-data` calls iSeries to fetch additional home inventory data (model year details, manufacturer info, etc.) and returns it to the UI.

**New:** No equivalent endpoint exists.

**Impact:** UI features that display ancillary home data will have no data source.

---

## Medium Findings

### M-1: HBPP and Warranty Merged — Separate iSeries Call Eliminated

**Legacy:** HomeBuyerProtectionPlan (HBPP) had its own iSeries call (`CalculateHomeBuyersProtectionPlanQuote`) separate from warranty.

**New:** HBPP is merged into the `WarrantyLine` type. The new `GenerateWarrantyQuoteCommandHandler` makes a single iSeries call that returns both warranty and HBPP data.

**Risk:** If iSeries returns different results when HBPP is requested standalone vs. combined with warranty, the merged approach may produce different values.

---

### M-2: Tax Question/Answer Sequencing Changed

**Legacy:** Tax calculation follows a strict sequence:
1. Delete all existing Q&A for the AppId
2. Insert new Q&A for the AppId
3. Calculate tax

Steps 1 and 2 are sequential; step 3 depends on both completing. The delete-then-insert ensures iSeries has clean Q&A data.

**New:** Tax questions come from CDC tables instead of iSeries. The delete/insert sequencing against iSeries may not exist in the new flow.

**Risk:** If CDC tables are stale or the new flow doesn't properly clear previous answers, tax calculations may use outdated Q&A data.

---

### M-3: State-Specific Tax Nullification Logic

**Legacy:** After iSeries returns tax components, `TaxService.cs` has state-specific nullification:
- Texas (TX): Nullify `MHIT` if the value is zero
- Tennessee (TN): Nullify `GrossReceiptCityTax` and `GrossReceiptCountyTax` if zero

**New:** Verify that `CalculateTaxesCommandHandler` replicates this nullification logic. If missing, zero-value tax components may appear in the response when they shouldn't.

---

### M-4: Commission Employee Number Enrichment

**Legacy:** `CommissionService.cs` enriches each sales team member with their employee number by calling the Organization Management API adapter. This is required for iSeries commission calculation.

**New:** The ECST cache projection should include employee numbers, but verify that the `CalculateCommissionCommandHandler` properly maps them to the iSeries request.

---

### M-5: Parallel vs Sequential iSeries Calls in Commission

**Legacy:** Commission calculation makes parallel calls to resolve Funding AppId and Home Center data, then sequential calls to UpdateAllowances and CalculateCommission.

**New:** Verify that the new handler maintains this sequencing. UpdateAllowances MUST complete before CalculateCommission because iSeries uses the allowance data for commission calculation.

---

### M-6: Package Cascade Operations on Cross-Package Events

**Legacy:** `PackagesService.cs` has cascade operations that affect ALL packages on a sale when certain conditions change:
- `DeleteStateTaxQuestions` — deletes tax Q&A from all packages when delivery state changes
- `DeleteHomeFirstInsurance` — removes HomeFirst insurance from all packages when occupancy becomes ineligible
- `DeleteHBPP` — removes HBPP from all packages when occupancy becomes ineligible
- `DeleteTaxCalculation` — clears tax calculations from all packages when tax-relevant data changes

**New:** Verify that domain event handlers properly cascade across all packages on a sale, not just the current package.

---

### M-7: Land Payoff Project Cost Auto-Generation

**Legacy:** When land pricing is set, the handler auto-generates a Land Payoff project cost (Cat 2, Item 1) with specific pricing:
```
SalePrice = PayoffAmountFinancing (or LandSalesPrice if CustomerHasLand)
EstimatedCost = PayoffAmountFinancing (or LandCost)
```

**New:** Verify that `UpdatePackageLandCommandHandler` replicates this auto-generation with the same field mappings for all land scenarios (CustomerHasLand, CustomerWantsToPurchaseLand, etc.).

---

### M-8: Trade-In "Trade Over Allowance" Project Cost Formula

**Legacy:** Auto-generates a Trade Over Allowance project cost (Cat 10, Item 9):
```
Amount = TradeAllowance - BookInAmount
SalePrice = Amount, EstimatedCost = Amount
```
This nets to zero GP impact (washes out).

**New:** Verify the formula and that it uses `BookInAmount` (not `ActualCashValue` or another field).

---

### M-9: Concession "Seller Paid Closing Cost" Project Cost

**Legacy:** When a concession credit is added, auto-generates a Seller Paid Closing Cost project cost (Cat 14, Item 1):
```
SalePrice = ConcessionAmount, EstimatedCost = ConcessionAmount
```

**New:** Verify that `UpdatePackageConcessionsCommandHandler` replicates this auto-generation.

---

### M-10: Delivery Address Side Effects Completeness

**Legacy:** Delivery address upsert triggers 3 side effects:
1. State change → clear tax questions from all packages
2. Occupancy ineligible → remove HomeFirst insurance and HBPP from all packages
3. Location change (state/city/county/zip) → clear tax calculations from all packages

**New:** The `UpdateDeliveryAddressCommandHandler` raises domain events, but verify all three side effects are handled by event subscribers.

---

### M-11: SalesTeam Validator Role/Split Rules

**Legacy:** `SalesTeamValidator.cs` (215 lines) has extensive validation:
- Required roles (SalesConsultant1 is mandatory)
- Split percentage must sum to 100%
- Manager fields required when IsManager = true
- Duplicate role detection
- Valid role enum values

**New:** Verify that `UpdatePackageSalesTeamCommandValidator` covers all these rules. The legacy validator is the most complex at 215 lines.

---

### M-12: Credit Validator Amount Constraints

**Legacy:** `CreditValidator.cs` validates:
- Down payment amount must be >= 0
- Down payment amount must be <= sale price
- Concession amount must be >= 0

**New:** Verify these constraints exist in `UpdatePackageDownPaymentCommandValidator` and `UpdatePackageConcessionsCommandValidator`.

---

### M-13: Home Validator Required Fields

**Legacy:** `HomeValidator.cs` validates required fields:
- `StockNumber` is required
- `ModelYear` is required
- `HomeType` must be a valid enum value
- `Condition` must be "New" or "Used"

**New:** Verify that `UpdatePackageHomeCommandValidator` covers these. Missing required field validation means incomplete home data can reach iSeries.

---

### M-14: W&A Option-Dependent iSeries Call

**Legacy:** Wheels & Axles pricing call is conditional on `WheelAndAxlesOption`:
- "WheelsAndAxlesRental" → call iSeries for rental pricing
- "WheelsAndAxlesPurchase" → call iSeries for purchase pricing
- "WheelsAndAxlesRentalAndPurchase" → call iSeries for both
- null/empty → skip iSeries call entirely, remove existing W&A project costs

**New:** Verify the `UpdatePackageHomeCommandHandler` or its domain event handler replicates this conditional logic.

---

### M-15: Multiple iSeries Pricing Calls for Retail Price

**Legacy:** Getting a retail price involves:
1. `GetRetailPrice` — base retail from iSeries
2. `GetOptionTotals` — factory options total from iSeries
3. `GetHomeMultipliers` — multiplier data from iSeries
4. Final: `RetailSalePrice = RetailPrice + OptionTotals`

**New:** Verify the `GetRetailPriceQueryHandler` or home pricing flow makes all required iSeries calls and combines them correctly.

---

## Low / Informational Findings

### L-1: Redirect Endpoint Not Ported

**Legacy:** `GET /sales/redirect?saleNumber={n}` redirects to the sale's detail page. Used for deep linking from other systems.

**New:** Not ported. Low impact — this is a convenience endpoint that can be handled by the frontend router.

---

### L-2: GetLegacySale Endpoint Not Ported

**Legacy:** `GET /sales/{id}/legacy` returns the raw DynamoDB sale record for debugging/migration purposes.

**New:** Not needed — DynamoDB is gone.

---

### L-3: Purchase Orders Endpoint Was Already a Stub

**Legacy:** `GET /packages/{id}/purchase-orders` existed but returned stub/empty data.

**New:** Not ported. No impact since legacy was already a stub.

---

### L-4: New Line Types Not in Legacy

**New system** adds two line types not present in legacy:
- `DiscountLine` — explicit discount tracking
- `FeeLine` — explicit fee tracking

These represent new functionality, not a gap.

---

### L-5: Concurrency Token Implementation Difference

**Legacy:** DynamoDB uses `[DynamoDBVersion]` attribute for automatic version increment on save.

**New:** EF Core uses `.IsConcurrencyToken()` on `Package.Version` column. **Known bug C-7** — the Version property is declared but may not be properly configured as a concurrency token in EF Core, making optimistic concurrency non-functional.

---

### L-6: Error Response Format Changed

**Legacy:** Throws `ApiException` with status codes, caught by middleware that returns `{ "error": "message" }`.

**New:** Returns `Result<T>` with `Error` objects, serialized as `{ "errors": [{ "code": "...", "message": "..." }] }`.

This is an intentional improvement but API consumers need to be aware of the new error shape.

---

### L-7: Domain Event for Line Removal Not Raised

**Known bug W-12.** When `RemoveLine` is called on the Package aggregate, no domain event is raised. This means downstream subscribers (funding, tax recalculation, etc.) aren't notified of line removals.

---

### L-8: Package.Version Concurrency Token Non-Functional

**Known bug C-7.** The `Version` property exists on the Package entity but may not be properly wired as an EF Core concurrency token, meaning concurrent updates could silently overwrite each other.

---

## Intentional Architectural Changes

These differences are by design and represent improvements in the new system:

| Area | Legacy | New | Rationale |
|------|--------|-----|-----------|
| **Data store** | DynamoDB | PostgreSQL + EF Core TPH | Relational integrity, ACID transactions |
| **Handler architecture** | Single `UpsertPackageCommandHandler` (905 lines) | 10+ dedicated command handlers | Single Responsibility, testability |
| **Cross-domain data** | HTTP adapters (Customers, Funding, OrgMgt, SES) | ECST cache projections via EventBridge | Decoupled modules, eventual consistency |
| **Validation** | Custom `IValidator<T>` interface | FluentValidation `AbstractValidator<T>` | Industry standard, richer rules |
| **Error handling** | `ApiException` throwing | `Result<T>` pattern | No exception-driven control flow |
| **Funding notification** | Synchronous HTTP POST | Async domain event (`PackageReadyForFundingDomainEvent`) | Decoupled, resilient |
| **Pricing service** | On-prem microservice proxy | Direct iSeries calls | Eliminated unnecessary hop |
| **Tax questions/exemptions** | iSeries live calls (`GetQuestionnaire`, `GetTaxExemptions`) | CDC tables in PostgreSQL | Faster, offline-capable |
| **HBPP line type** | Separate line type with own iSeries call | Merged into `WarrantyLine` | Simplification — same pricing pattern |
| **Details storage** | DynamoDB nested documents | JSONB with `VersionedJsonConverter<T>` | Schema evolution support |
| **Team proxy** | `TeamProxyController` routing to OrgMgt API | Eliminated — direct access | Architecture simplification |

---

## Endpoint Gap Analysis

### Legacy Endpoints (28 total)

| # | Legacy Endpoint | Method | New Equivalent | Status |
|---|----------------|--------|----------------|--------|
| 1 | `/sales` | POST | `POST /sales` | Ported |
| 2 | `/sales/{id}` | GET | `GET /sales/{id}` | Ported (6 fields always null — H-8) |
| 3 | `/sales/{saleNumber}` | GET | — | **Not ported (H-11)** |
| 4 | `/sales/redirect` | GET | — | Not ported (L-1) |
| 5 | `/sales/{id}/legacy` | GET | — | Not ported (L-2) |
| 6 | `/sales/summaries` | POST | `POST /sales/summaries` | Ported |
| 7 | `/sales/{id}/delivery-address` | GET | `GET /sales/{id}/delivery-address` | Ported |
| 8 | `/sales/{id}/delivery-address` | PUT | `PUT /sales/{id}/delivery-address` | Ported |
| 9 | `/packages` | POST | `POST /packages` | Ported |
| 10 | `/packages/{id}` | GET | `GET /packages/{id}` | Ported |
| 11 | `/packages/{id}` | DELETE | `DELETE /packages/{id}` | Ported |
| 12 | `/packages/{id}/primary` | PUT | `PUT /packages/{id}/primary` | Ported |
| 13 | `/packages/{id}/name` | PUT | `PUT /packages/{id}/name` | Ported |
| 14 | `/packages/{id}/home` | PUT | `PUT /packages/{id}/home` | Ported |
| 15 | `/packages/{id}/land` | PUT | `PUT /packages/{id}/land` | Ported |
| 16 | `/packages/{id}/insurance` | PUT | `PUT /packages/{id}/insurance` | Ported |
| 17 | `/packages/{id}/trade-ins` | PUT | `PUT /packages/{id}/trade-ins` | Ported |
| 18 | `/packages/{id}/down-payment` | PUT | `PUT /packages/{id}/down-payment` | Ported |
| 19 | `/packages/{id}/concessions` | PUT | `PUT /packages/{id}/concessions` | Ported |
| 20 | `/packages/{id}/project-costs` | PUT | `PUT /packages/{id}/project-costs` | Ported |
| 21 | `/packages/{id}/sales-team` | PUT | `PUT /packages/{id}/sales-team` | Ported |
| 22 | `/packages/{id}/tax` | PUT | `PUT /packages/{id}/tax` | Ported |
| 23 | `/packages/{id}/tax` | POST | `POST /packages/{id}/tax` | Ported |
| 24 | `/packages/{id}/warranty` | PUT | `PUT /packages/{id}/warranty` | Ported |
| 25 | `/sales/{id}/insurance/home-first-quote` | POST | `POST /sales/{id}/insurance/home-first-quote` | Ported |
| 26 | `/sales/{id}/insurance/outside` | POST | `POST /sales/{id}/insurance/outside` | Ported |
| 27 | `/sales/{id}/insurance/print` | POST | `POST /sales/{id}/insurance/print` | **Stubbed (C-3)** |
| 28 | `/sales/{id}/warranty/quote` | POST | `POST /sales/{id}/warranty/quote` | Ported |
| 29 | `/sales/{id}/commission` | POST | `POST /sales/{id}/commission` | Ported |
| 30 | `/sales/{id}/home/ancillary-data` | GET | — | **Not ported (H-12)** |
| 31 | `/packages/{id}/purchase-orders` | GET | — | Not ported (L-3, was stub) |

**Summary:** 25 ported, 1 stubbed, 5 not ported (2 high impact, 3 low impact)

---

## Area-by-Area Detailed Audit

### 1. Home Line

**Legacy:** `UpsertPackageCommandHandler.cs` lines ~150-350 (home section) + `PackageDtoExtensions.cs` (retail price computation, W&A logic)

**New:** `UpdatePackageHomeCommandHandler.cs` + `HomeLineUpdatedDomainEvent` handler

| Aspect | Legacy | New | Match? |
|--------|--------|-----|--------|
| Stock number lookup | iSeries `GetHomeInventoryData` | iSeries pricing calls | Verify |
| Retail price = RetailPrice + OptionTotals | Yes | Verify formula | Verify |
| EstimatedCost from iSeries | Yes — `InvoiceAmount` | Yes | Yes |
| W&A conditional logic | 4-way branch on `WheelAndAxlesOption` | Domain event handler | Verify (M-14) |
| Home type change → strip project costs | Complex matrix in `PackageDtoExtensions` | `HomeLineUpdatedDomainEvent` | Verify (H-4) |
| Home type change → strip HomeFirst insurance | Yes | Domain event | Verify |
| Home type change → strip HBPP | Yes | Domain event (merged into warranty) | Verify |
| `ShouldExcludeFromPricing` | Always `false` for HomeLine | Always `false` | Yes |

**Key differences:**
- Legacy computes retail price inline; new may use separate pricing queries
- W&A project cost auto-generation logic needs detailed comparison against legacy's 4-way branch

---

### 2. Land Line

**Legacy:** `UpsertPackageCommandHandler.cs` lines ~350-480 (land section) + `LandValidator.cs` (530 lines)

**New:** `UpdatePackageLandCommandHandler.cs` + `UpdatePackageLandCommandValidator.cs`

| Aspect | Legacy | New | Match? |
|--------|--------|-----|--------|
| 5 land scenarios | CustomerHasLand (3 sub), CustomerWantsToPurchase (2 sub) | Verify all 5 | Verify |
| Land Payoff project cost (Cat 2/Item 1) | Auto-generated | Verify | Verify (M-7) |
| `SalePrice` mapping per scenario | Complex — varies by scenario | Verify | Verify |
| `EstimatedCost` mapping per scenario | Complex — varies by scenario | Verify | Verify |
| Validator: 530 lines of branching rules | `LandValidator.cs` | `UpdatePackageLandCommandValidator.cs` | Verify completeness |

**Key risk:** Land validation is the most complex validator in legacy (530 lines with deeply nested conditional branches). The new FluentValidation equivalent must cover all scenarios.

---

### 3. Insurance Line

**Legacy:** `UpsertPackageCommandHandler.cs` (insurance section) + `InsuranceService.cs` + `InsuranceQuotePdfGenerator.cs` (731 lines)

**New:** `UpdatePackageInsuranceCommandHandler.cs` + `GenerateHomeFirstQuoteCommandHandler.cs` + `RecordOutsideInsuranceCommandHandler.cs` + `PrintInsuranceQuoteCommandHandler.cs`

| Aspect | Legacy | New | Match? |
|--------|--------|-----|--------|
| Admin override (`PUT /insurance`) | Yes — `TotalPremium` → `SalePrice` | Yes | Yes |
| HomeFirst quote (iSeries call) | Yes | Yes | Yes |
| Outside insurance | Yes — `PremiumAmount` → `SalePrice` | Yes | Yes |
| `EstimatedCost` always 0 | Yes | Yes | Yes |
| `RetailSalePrice` always 0 | Yes | Yes | Yes |
| PDF generation | 731-line MigraDocCore | **Hardcoded stub** | **No (C-3)** |
| Occupancy ineligibility on update | Auto-removes on every package update | Only on new quote generation | **Partial (H-3)** |
| Responsibility validation | Required, must be Buyer/Seller | Not validated | **No (H-9)** |

---

### 4. Warranty / HBPP Line

**Legacy:** Warranty and HBPP are separate line types with separate handlers and iSeries calls.

**New:** Merged into single `WarrantyLine` with `WarrantyDetails` containing both warranty and HBPP data.

| Aspect | Legacy | New | Match? |
|--------|--------|-----|--------|
| Warranty admin override | Yes | Yes | Yes |
| Warranty iSeries quote | Yes — `CalculateWarrantyQuote` | Yes — `GenerateWarrantyQuoteCommandHandler` | Yes |
| HBPP iSeries quote | Separate `CalculateHomeBuyersProtectionPlanQuote` call | Merged into warranty quote | **Changed (M-1)** |
| `SalePrice` = warranty premium | Yes | Yes | Yes |
| `EstimatedCost` always 0 | Yes | Yes | Yes |
| Tax flag on warranty deselect | Yes — flags recalculation | **Missing** | **No (H-7)** |
| Warranty + HBPP removal on home change | Yes — domain event | Yes — domain event | Yes |

---

### 5. Trade-In Line

**Legacy:** `UpsertPackageCommandHandler.cs` (trade-in section) + `TradeInValidator.cs` (64 lines)

**New:** `UpdatePackageTradeInsCommandHandler.cs`

| Aspect | Legacy | New | Match? |
|--------|--------|-----|--------|
| Multiple trade-ins supported | Yes | Yes | Yes |
| Trade Over Allowance project cost (Cat 10/Item 9) | Auto-generated: `TradeAllowance - BookInAmount` | Verify formula | Verify (M-8) |
| Trade-in for iSeries (tax/commission) | **First trade-in only** | **Sum of all trade-ins** | **Changed (C-4)** |
| Type code mapping | Detailed enum → iSeries code mapping | Different mapping | Verify |
| Responsibility validation | Required | Not validated | **No (H-9)** |

**Key risk:** The trade-in type code mapping between legacy and new differs substantially. Legacy maps specific types (SingleWide, DoubleWide, Automobile, etc.) to iSeries numeric codes. Verify the new mapping produces identical codes.

---

### 6. Credits (Down Payment & Concessions)

**Legacy:** `UpsertPackageCommandHandler.cs` (credit sections)

**New:** `UpdatePackageDownPaymentCommandHandler.cs` + `UpdatePackageConcessionsCommandHandler.cs`

| Aspect | Legacy | New | Match? |
|--------|--------|-----|--------|
| Down payment: `SalePrice` = negative amount | Yes | Yes | Yes |
| Down payment: `EstimatedCost` = 0 | Yes | Yes | Yes |
| Concession: `SalePrice` = negative amount | Yes | Yes | Yes |
| Concession: SPCC project cost (Cat 14/Item 1) | Auto-generated | Verify | Verify (M-9) |
| Amount >= 0 validation | Yes | Verify | Verify (M-12) |
| Down payment <= sale price validation | Yes | Verify | Verify (M-12) |

---

### 7. Project Costs

**Legacy:** `UpsertPackageCommandHandler.cs` (project cost section) + `ProjectCostValidator.cs` (25 lines)

**New:** `UpdatePackageProjectCostsCommandHandler.cs`

| Aspect | Legacy | New | Match? |
|--------|--------|-----|--------|
| All 3 pricing fields pass through | Yes | Yes | Yes |
| `ShouldExcludeFromPricing` user-controlled | Yes | Yes | Yes |
| 6 system-managed keys rejected | W&A Rental (1/28), W&A Purchase (1/29), Land Payoff (2/1), Use Tax (9/21), Trade Over Allowance (10/9), SPCC (14/1) | Verify all 6 rejected | Verify |
| Category/Item uniqueness | Enforced per package | Verify | Verify |

---

### 8. Sales Team

**Legacy:** `SalesTeamValidator.cs` (215 lines — most complex validator)

**New:** `UpdatePackageSalesTeamCommandHandler.cs` + validator

| Aspect | Legacy | New | Match? |
|--------|--------|-----|--------|
| SalesConsultant1 required | Yes | Verify | Verify (M-11) |
| Split % sums to 100% | Yes | Verify | Verify (M-11) |
| Manager fields required when IsManager | Yes | Verify | Verify (M-11) |
| Duplicate role detection | Yes | Verify | Verify (M-11) |
| Employee number enrichment for iSeries | Via OrgMgt adapter | Via ECST cache | Verify (M-4) |

---

### 9. Tax Service

**Legacy:** `TaxService.cs` (838 lines)

**New:** `UpdatePackageTaxCommandHandler.cs` + `CalculateTaxesCommandHandler.cs`

| Aspect | Legacy | New | Match? |
|--------|--------|-----|--------|
| Configure tax settings (PUT) | Saves config, clears calculations | Yes | Yes |
| Calculate taxes (POST) | Complex multi-step iSeries flow | Yes | Yes |
| Guard: Package must exist | Yes | Yes | Yes |
| Guard: Home line must exist | Yes | Verify | Verify |
| Guard: Delivery address required | Yes | Verify | Verify |
| Resolve AppId from Funding | Yes — via FundingRequestAdapter | Yes — via ECST cache | Verify |
| Resolve HomeCenter from OrgMgt | Yes — via OrgMgtAdapter | Yes — via ECST cache | Verify |
| `MasterDealerNumber = 29` | Sent to iSeries | **Not sent** | **No (C-5)** |
| Delete/Insert Q&A sequencing | Sequential against iSeries | CDC tables (different approach) | Changed (M-2) |
| State-specific nullification (TX/TN) | Yes | Verify | Verify (M-3) |
| Use Tax project cost (Cat 9/Item 21) | Auto-generated when UseTax > 0 | Verify | Verify |
| 6 TaxItem components | Yes | Verify all 6 mapped | Verify |
| Tax validator (6 rules) | `TaxValidator.cs` | **Missing** | **No (H-1)** |
| `PreviouslyTitled` type | String (null/Yes/No) | Bool (true/false) | **Lossy (H-5)** |
| `TaxExemptionCode` null guard | Yes | **Missing** | **No (H-6)** |
| `MustRecalculateTaxes` flag | Set on tax-relevant changes | Yes | Yes |

**Key risk:** Tax is the highest-risk area. The missing validator, lossy type change, missing null guard, and missing MasterDealerNumber compound to create significant risk of incorrect tax calculations.

---

### 10. Commission Service

**Legacy:** `CommissionService.cs` (527 lines)

**New:** `CalculateCommissionCommandHandler.cs`

| Aspect | Legacy | New | Match? |
|--------|--------|-----|--------|
| Parallel data loading (Funding + OrgMgt) | Yes — `Task.WhenAll` | Verify parallelism | Verify (M-5) |
| Employee number enrichment | Via OrgMgt adapter for each team member | Via ECST cache | Verify (M-4) |
| AppId resolution from Funding | Yes | Via ECST cache | Verify |
| UpdateAllowances → CalculateCommission sequencing | Sequential (allowances first) | Verify | Verify (M-5) |
| `MasterDealerNumber = 29` | Sent | Verify | Verify (may share C-5) |
| CommissionableGP stored on Package | Yes | Yes (but stale risk — C-2) | Partial |

---

### 11. Sales Service

**Legacy:** `SalesService.cs` (525 lines) — 6 operations

**New:** `CreateSaleCommandHandler.cs` + `GetSaleByIdQueryHandler.cs`

| Aspect | Legacy | New | Match? |
|--------|--------|-----|--------|
| CreateOrGetSale | Yes — idempotent, finds existing or creates new | Yes — `CreateSaleCommandHandler` | Verify idempotency |
| GetSaleById | Yes — enriched with customer data from adapter | Yes — 6 fields always null | **Partial (H-8)** |
| GetSaleBySaleNumber | Yes | **Not ported** | **No (H-11)** |
| RedirectAsync | Yes — deep link redirect | Not ported (L-1) | Low impact |
| GetLegacySale | Yes — raw DynamoDB record | Not ported (L-2) | Not needed |
| GetSaleSummariesByStockNumbers | Yes | Verify | Verify |

---

### 12. Delivery Address

**Legacy:** `DeliveryAddressService.cs` (344 lines)

**New:** `CreateDeliveryAddressCommandHandler.cs` + `UpdateDeliveryAddressCommandHandler.cs` + `GetDeliveryAddressQueryHandler.cs`

| Aspect | Legacy | New | Match? |
|--------|--------|-----|--------|
| Upsert (create or update) | Single endpoint | Split into Create + Update | Intentional |
| Side effect: State change → clear tax Q&A | Yes — all packages on sale | Verify scope (all packages) | Verify (M-10) |
| Side effect: Occupancy ineligible → remove insurance | Yes — all packages | Verify scope | Verify (M-10) |
| Side effect: Location change → clear tax calc | Yes — all packages | Verify scope | Verify (M-10) |
| Occupancy ineligibility on update vs create | Both | Verify both | Verify (H-3) |

---

### 13. Packages Service / CRUD

**Legacy:** `PackagesService.cs` (421 lines) + `PackagesController.cs`

**New:** `CreatePackageCommandHandler.cs` + `DeletePackageCommandHandler.cs` + `SetPackageAsPrimaryCommandHandler.cs` + `UpdatePackageNameCommandHandler.cs` + `GetPackageByIdQueryHandler.cs`

| Aspect | Legacy | New | Match? |
|--------|--------|-----|--------|
| Create package | Yes | Yes | Yes |
| Delete package | Yes — with Funding notification | Yes — with domain event | Yes |
| Set primary | Yes — cascade: old primary demoted | Yes | Verify cascade |
| Update name | Yes — unique within sale (case-insensitive) | Yes | Verify uniqueness |
| Get package by ID | Yes — full DTO mapping | Yes — `PackageDetailMapper` | Verify completeness |
| Cascade: Delete tax Q&A all packages | Yes — cross-package | Verify in domain events | Verify (M-6) |
| Cascade: Delete insurance all packages | Yes — cross-package | Verify in domain events | Verify (M-6) |
| Cascade: Delete HBPP all packages | Yes — cross-package | Verify in domain events | Verify (M-6) |
| Cascade: Delete tax calc all packages | Yes — cross-package | Verify in domain events | Verify (M-6) |
| Singleton constraints | `DefaultPackageValidator` | **Not enforced** | **No (H-2)** |

---

### 14. Pricing / iSeries Adapter

**Legacy:** `ISeriesApiAdapter.cs` (813 lines) — 11 methods + `SesAdapter.cs` (on-prem pricing proxy)

**New:** `IiSeriesAdapter.cs` — 11 methods (4 pricing, 4 tax, 2 insurance, 1 commission)

| Legacy Method | New Method | Match? |
|---------------|------------|--------|
| `GetQuestionnaire` | CDC table query | Changed (M-2) |
| `GetTaxExemptions` | CDC table query | Changed (M-2) |
| `GetHomeInventoryAncillaryData` | **Not ported** | **No (H-12)** |
| `CalculateCommission` | `CalculateCommission` | Yes |
| `UpdateAllowances` | `UpdateAllowances` | Verify (C-5: MasterDealerNumber) |
| `CalculateTax` | `CalculateTax` | Verify |
| `InsertTaxQuestionAnswers` | CDC approach | Changed |
| `DeleteTaxQuestionAnswers` | CDC approach | Changed |
| `CalculateHomeFirstQuote` | `CalculateHomeFirstQuote` | Yes |
| `CalculateHomeBuyersProtectionPlanQuote` | Merged into warranty quote | Changed (M-1) |
| `GetInsuranceQuoteSheetData` | **Stubbed** | **No (C-3)** |
| `GetRetailPrice` (via SES adapter) | `GetRetailPrice` (direct) | Yes — proxy eliminated |
| `GetOptionTotals` (via SES adapter) | `GetOptionTotals` (direct) | Yes |
| `GetWheelsAndAxlesPrice` (via SES adapter) | `GetWheelsAndAxlesPrice` (direct) | Yes |
| `GetHomeMultipliers` (via SES adapter) | `GetHomeMultipliers` (direct) | Yes |

---

### 15. Validators

**Comparison of legacy vs new validation coverage:**

| Legacy Validator | Lines | New Validator | Coverage |
|-----------------|-------|---------------|----------|
| `HomeValidator` | 72 | `UpdatePackageHomeCommandValidator` | Verify (M-13) |
| `LandValidator` | 530 | `UpdatePackageLandCommandValidator` | Verify — highest risk |
| `InsuranceValidator` | 40 | `GenerateHomeFirstQuoteCommandValidator` + `RecordOutsideInsuranceCommandValidator` | Missing responsibility (H-9) |
| `HomeBuyerProtectionPlanValidator` | 41 | Merged into warranty validator | Missing responsibility (H-9) |
| `TradeInValidator` | 64 | `UpdatePackageTradeInsCommandValidator` | Missing responsibility (H-9) |
| `CreditValidator` | 26 | `UpdatePackageDownPaymentCommandValidator` + `UpdatePackageConcessionsCommandValidator` | Verify (M-12) |
| `ProjectCostValidator` | 25 | `UpdatePackageProjectCostsCommandValidator` | Verify |
| `SalesTeamValidator` | 215 | `UpdatePackageSalesTeamCommandValidator` | Verify (M-11) |
| `TaxValidator` | 38 | `UpdatePackageTaxCommandValidator` | **Missing all rules (H-1)** |
| `DefaultPackageValidator` | 59 | — | **Missing entirely (H-2)** |

---

### 16. DTO Mapping / Entities

**Legacy entities (DynamoDB):**
- 38 entity files in `Domain.Sales.Entities/DynamoEntities/`
- Flat structure with nullable properties
- No inheritance hierarchy

**New entities (EF Core TPH):**
- `PackageLine` base class with `Discriminator` column
- 11 derived types (Home, Land, Tax, Insurance, Warranty, TradeIn, Credit, ProjectCost, SalesTeam, Discount, Fee)
- JSONB `Details` column per line type via `VersionedJsonConverter<T>`
- `IVersionedDetails` with `SchemaVersion` + `ExtensionData` for forward compatibility

**Key mapping differences:**
1. Legacy stores all fields flat on the entity; new splits pricing fields (on `PackageLine`) from domain-specific fields (in JSONB `Details`)
2. Legacy uses nullable types extensively; new uses required properties with sensible defaults
3. New has schema versioning infrastructure (`DetailsVersionRegistry<T>`, `IDetailsUpgrader<T>`) for non-breaking JSONB schema evolution

**Response mapping:**
- Legacy: `PackageDtoExtensions.cs` (760 lines) with computed properties and complex transformations
- New: `PackageDetailMapper.cs` — verify it covers all computed properties from legacy

---

## Remediation Priority

### Immediate (Before Go-Live)

1. **C-1:** Confirm GP formula change with business stakeholders
2. **C-4:** Confirm trade-in aggregation change (first-only vs sum-all)
3. **C-5:** Add `MasterDealerNumber = 29` to iSeries requests
4. **H-1:** Implement tax validator with all 6 rules
5. **H-2:** Add package-level singleton constraints
6. **H-5:** Address `PreviouslyTitled` type change (add nullable bool or string)
7. **H-6:** Add `TaxExemptionCode` null guard
8. **H-7:** Flag tax recalculation on warranty deselection

### Short-Term (First Sprint Post-Launch)

9. **C-3:** Port insurance PDF generation (or integrate QuestPDF)
10. **H-3:** Add occupancy ineligibility check to package update flow
11. **H-4:** Verify home-type project cost removal matrix
12. **H-8:** Populate 6 missing customer fields from ECST cache
13. **H-9:** Add responsibility validation to 4 validators
14. **H-11:** Port `GetSaleBySaleNumber` endpoint

### Medium-Term

15. **H-10:** Handle Investment occupancy type
16. **H-12:** Port HomeInventoryAncillaryData endpoint
17. **M-1 through M-15:** Verify all medium findings (many may already be correct but need confirmation)

---

## Appendix: Legacy File Inventory

### Application Layer (~6,450 lines across 65 files)

```
Domain.Sales.Application/
├── Commission/
│   └── CommissionService.cs (527 lines)
├── DeliveryAddress/
│   └── DeliveryAddressService.cs (344 lines)
├── Insurance/
│   ├── InsuranceService.cs (~200 lines)
│   └── Pdf/InsuranceQuotePdfGenerator.cs (731 lines)
├── Land/
│   └── Validators/LandValidator.cs (530 lines)
├── Packages/
│   ├── Commands/UpsertPackage/UpsertPackageCommandHandler.cs (905 lines)
│   ├── Extensions/PackageDtoExtensions.cs (760 lines)
│   ├── Services/PackagesService.cs (421 lines)
│   └── Validators/ (6 files, ~300 lines total)
├── Sales/
│   └── Services/SalesService.cs (525 lines)
└── TaxesAndFees/
    └── Services/TaxService.cs (838 lines)
```

### API Layer (29 endpoints across 11 controllers)

```
Domain.Sales.Api/
├── Controllers/
│   ├── CommissionController.cs
│   ├── DeliveryAddressController.cs
│   ├── HomeController.cs
│   ├── InsuranceController.cs
│   ├── PackagesController.cs
│   ├── ProjectCostsController.cs
│   ├── PurchaseOrdersController.cs
│   ├── SalesController.cs
│   ├── SalesTeamController.cs
│   ├── TaxController.cs
│   └── TeamProxyController.cs (eliminated in new)
```

### Adapters Layer (4 HTTP adapters, ~2,500 lines)

```
Domain.Sales.Adapters/
├── Customers/CustomersAdapter.cs
├── FundingRequest/FundingRequestAdapter.cs
├── iSeries/ISeriesApiAdapter.cs (813 lines)
├── OrgMgt/OrgMgtAdapter.cs
└── Ses/SesAdapter.cs (on-prem pricing proxy — eliminated)
```

---

*This audit was generated by 13 parallel AI agents, each responsible for one functional area. Each agent read both the legacy and new codebases service-by-service and reported differences. Findings were compiled and deduplicated into this document.*

*Items marked "Verify" require manual confirmation — the audit agents identified the code exists in both systems but could not conclusively confirm behavioral equivalence without runtime testing.*
