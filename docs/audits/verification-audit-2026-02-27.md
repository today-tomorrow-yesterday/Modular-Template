# AI-Assisted Verification Audit — 2026-02-27

Findings from executing the playbooks in `docs/testing/ai-assisted-verification.md` against the live API (`http://localhost:5004`) and database (`sales_dev`).

---

## Finding 1: Seeder sets Home/Land Responsibility to Buyer — Handlers set Seller

**Severity:** Low (data correctness on fresh seed only; handlers correct it on first use)

**Current Code:**
- `PackageLineFakers.cs:146` — `responsibility: Responsibility.Buyer` (Home)
- `PackageLineFakers.cs:243` — `responsibility: Responsibility.Buyer` (Land)

**Handler Code:**
- `UpdatePackageHomeCommandHandler.cs:196` — `responsibility: Responsibility.Seller`
- `UpdatePackageLandCommandHandler.cs:155` — `responsibility: Responsibility.Seller`

**Observed:**
- API GET on seeded data returns `"responsibility": "Buyer"` for Home and Land
- After first PUT (handler invocation), responsibility flips to `"Seller"`

**Legacy System Evidence:**
- In the legacy Sales API, the Home is the seller's asset — the seller owns the manufactured home and sells it to the buyer. `Responsibility = Seller`.
- Land follows the same logic: the seller provides the land (whether home-center-owned or customer payoff).
- The target journey docs (`01-HOME-JOURNEY.md`, `02-LAND-JOURNEY.md`) confirm this.

**Suggested Fix:**
```csharp
// PackageLineFakers.cs:146
responsibility: Responsibility.Seller,  // was Buyer

// PackageLineFakers.cs:243
responsibility: Responsibility.Seller,  // was Buyer
```

**Why This Matters:**
- Any verification of seeded data will see `Buyer` and flag it as wrong
- First handler invocation silently flips the value, masking the seeder error
- Downstream consumers reading seeded-but-unmodified packages get incorrect responsibility

---

## Finding 2: Seeded Gross Profit is always 0

**Severity:** Medium (misleading seed data; could confuse new developers)

**Current Code:**
- `SalesModuleSeeder.cs:163` — `db.PackageLines.AddRange(allLines)` — adds lines via EF directly
- `Package.cs:49` — `GrossProfit = 0m` in `Package.Create()`
- `Package.cs:86` — `RecalculateGrossProfit()` only called inside `AddLine()` / `RemoveLine()`

**Observed:**
- All 26 seeded packages have `grossProfit = 0` despite having non-zero priced lines
- Example: Package `ab743b1f` has Home (440K-375K=65K) + Warranty (2K-1K=1K) + 2 ProjectCosts (500 each) = expected GP 67000, actual GP = 0
- After first handler PUT, GP is correctly recalculated (confirmed: package `b7a835a0` went from 0 to 96000)

**Root Cause:**
The seeder creates package lines via `db.PackageLines.AddRange()`, bypassing the `Package.AddLine()` aggregate method that triggers `RecalculateGrossProfit()`. The Package aggregate never gets a chance to compute GP from the seeded lines.

**Legacy System Evidence:**
The legacy system always had GP computed — it was calculated server-side on every save. The new system correctly computes GP in handlers, but the seeder shortcut skips this step.

**Suggested Fix — Option A (preferred):**
After inserting all package lines, loop through packages and call `RecalculateGrossProfit()`:
```csharp
// After line 163 in SalesModuleSeeder.cs
foreach (var package in packages)
{
    var packageLines = allLines.Where(l => l.PackageId == package.Id).ToList();
    // Use reflection or a friend method to populate _lines, then recalculate
}
```

**Suggested Fix — Option B (simpler):**
Compute GP directly from the lines and set it via reflection or a dedicated seeder method:
```csharp
foreach (var package in packages)
{
    var gp = allLines
        .Where(l => l.PackageId == package.Id && !l.ShouldExcludeFromPricing)
        .Sum(l => l.SalePrice - l.EstimatedCost);
    // Set GP via reflection or dedicated method
}
```

**Caveat for Option B:** The `ShouldExcludeFromPricing` virtual property issue (Finding 3) means the base property getter would need to be used carefully — see Finding 3.

---

## Finding 3: DB column `should_exclude_from_pricing` is stale for 4 line types

**Severity:** Medium (DB untrustworthy for SQL-based verification of these types)

**Current Code:**
- `PackageLine.cs:26` — `public virtual bool ShouldExcludeFromPricing { get; protected set; }` (base, auto-property)
- `CreditLine.cs:8` — `public override bool ShouldExcludeFromPricing => true;` (getter-only override)
- `SalesTeamLine.cs:7` — `public override bool ShouldExcludeFromPricing => true;`
- `TradeInLine.cs:7` — `public override bool ShouldExcludeFromPricing => true;`
- `LandLine.cs:10` — `public override bool ShouldExcludeFromPricing => true;`

**Observed (DB vs API):**

| Line Type | DB `should_exclude_from_pricing` | API `shouldExcludeFromPricing` | Correct Value |
|-----------|----------------------------------|-------------------------------|---------------|
| Home | false | false | false |
| Land | **false** | **true** | **true** |
| Tax | true | true | true |
| Insurance | true | true | true |
| Warranty | false | false | false |
| TradeIn | **false** | **true** | **true** |
| ProjectCost | false | false | false |
| Credit | **false** | **true** | **true** |
| SalesTeam | **false** | **true** | **true** |

**Root Cause:**
These 4 types use expression-bodied getter overrides (`=> true`) which never set the backing field of the base class auto-property. EF Core persists the backing field value (default `false`) to the DB column. When reading, EF Core populates the backing field from the DB, but the overridden getter ignores it and returns `true`.

This is NOT just a seeder issue — it affects ALL persistence. Even when handlers create these line types (e.g., Land Payoff project cost creation uses `shouldExcludeFromPricing: true` explicitly, which works because `ProjectCostLine` doesn't override the property), the LandLine, TradeInLine, CreditLine, and SalesTeamLine types will always persist `false` to the DB while the domain correctly returns `true`.

**Post-Mutation DB Verification (confirmed):**
After running land handler PUT on package `b7a835a0`, the DB shows:
```
 Home       | Seller | f (correct)
 Land       | Seller | f (WRONG — domain returns true)
 Credit     | Seller | f (WRONG — domain returns true)
 Sales Team |        | f (WRONG — domain returns true)
 Trade In   | Seller | f (WRONG — domain returns true)
 Tax        |        | t (correct)
 Warranty   | Seller | f (correct)
```
This proves the bug persists through ALL code paths — not just seeding, but handler mutations too.

**In-Process Impact:** None — the domain override is authoritative, and the API always reads through the domain.

**SQL/External Impact:**
- SQL verification queries checking `should_exclude_from_pricing` get wrong results for these 4 types
- Any external system reading the DB directly would see incorrect exclusion flags
- Reporting queries would be inaccurate

**Legacy System Evidence:**
In the legacy Sales API, these flags were stored correctly in the database because they were explicitly set on every write. The new system's C# property override pattern is elegant for in-process correctness but creates a persistence gap.

**Suggested Fix:**
Set the backing field in the `Create()` factory methods of each affected type:
```csharp
// LandLine.Create()
return new LandLine
{
    ...
    ShouldExcludeFromPricing = true,  // Add this — currently missing
    ...
};

// TradeInLine.Create()
return new TradeInLine
{
    ...
    ShouldExcludeFromPricing = true,  // Add this
    ...
};

// CreditLine.CreateDownPayment() and CreateConcession()
ShouldExcludeFromPricing = true,  // Add this

// SalesTeamLine.Create()
ShouldExcludeFromPricing = true,  // Add this
```

The `protected set` on the base class allows derived types to set it. The override getter still provides the safety net, but the DB value would now be correct.

---

## Finding 4: iSeries W&A call crashes the home handler for single-section homes

**Severity:** Medium (single-section homes with W&A option cannot be saved in dev)

**Current Code:**
- `UpdatePackageHomeCommandHandler.cs:267-343` — Step 5 `RecalculateWheelAndAxlePricing()`
- The method calls `iSeriesAdapter.GetWheelAndAxlePriceByStock()` or `CalculateWheelAndAxlePriceByCount()` without try-catch
- iSeries is stubbed at `http://localhost:9999` (connection refused)

**Observed:**
- PUT home with `numberOfFloorSections: 1` + `wheelAndAxlesOption: 1` (Purchase) + `homeSourceType: 0` (OnLot) → **500 Internal Server Error**
- PUT home with `numberOfFloorSections: 2` (multi-section, skips W&A) → **200 OK**, GP calculated correctly

**Expected Behavior:**
The handler should either:
1. Gracefully handle iSeries failures (try-catch, skip W&A, log warning)
2. Or the resilience middleware should catch the connection failure and return a structured error

**Current Behavior:**
The exception propagates through the Polly resilience pipeline (retry 2x with 500ms delay, then fail after timeout), resulting in an unhandled exception and a generic 500 error.

**Legacy System Evidence:**
The legacy Sales API integrated directly with the iSeries for W&A pricing. In production, this call succeeds. In development, the legacy system either had a local iSeries mock or handled the failure gracefully with a default value.

**Impact:**
- In dev, any home update for single-section homes with W&A option will fail
- The playbook must use multi-section homes or null W&A option to test home handler mutations
- This is documented in the playbook with the ⚡ iSeries marker

**Suggested Fix:**
Wrap the iSeries calls in a try-catch that logs the failure and skips W&A pricing:
```csharp
try
{
    waResult = await iSeriesAdapter.GetWheelAndAxlePriceByStock(...);
}
catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
{
    logger.LogWarning(ex, "W&A pricing unavailable — skipping");
    return; // No W&A project cost created
}
```

---

## Finding 5: Playbook document had incorrect ShouldExcludeFromPricing values [RESOLVED]

**Severity:** Document error (corrected in updated document)
**Status:** Resolved — playbook tables updated with correct domain override values.

The original playbook document incorrectly listed:
- Land: `shouldExcludeFromPricing = false` — **Correct value: `true`** (domain override; land never contributes to GP)
- SalesTeam: `shouldExcludeFromPricing = false` — **Correct value: `true`** (domain override; metadata-only, all-zero prices)

**Source of Error:**
The plan was derived from the target journey docs which describe the "expected" flag values. The domain code uses property overrides that are the actual source of truth.

The playbook document has been updated to reflect the correct values per the actual domain code.

---

## Finding 6: Project cost pruning matrix was incomplete in the plan [RESOLVED]

**Severity:** Document error (corrected in updated document)
**Status:** Resolved — pruning matrix updated with all items including TaxUndercollection (13/98) and the Refurbishment/Drapes vs Decorating/DecoratingDrapes distinction.

**Plan said:**

| Home Type | Removes |
|-----------|---------|
| New | Repo Costs (Cat 12/*) |
| Used | Decorating Drapes (Cat 15/4), Repo Costs (Cat 12/*) |
| Repo | Refurb Cleaning (Cat 11/1), RepairRefurb (Cat 11/2), RefurbParts (Cat 11/3), Decorating Drapes (Cat 15/4) |

**Handler code actually does** (`UpdatePackageHomeCommandHandler.cs:231-253`):

| Home Type | Removes |
|-----------|---------|
| New | Refurbishment/Cleaning (11/1), Refurbishment/RepairRefurb (11/2), Refurbishment/RefurbParts (11/3), Refurbishment/Drapes (11/4), ALL RepoCosts (Cat 12), MiscTax/TaxUndercollection (13/98) |
| Used | ALL RepoCosts (Cat 12), Decorating/DecoratingDrapes (15/4), MiscTax/TaxUndercollection (13/98) |
| Repo | Refurbishment/Cleaning (11/1), Refurbishment/RepairRefurb (11/2), Refurbishment/RefurbParts (11/3), Decorating/DecoratingDrapes (15/4), MiscTax/TaxUndercollection (13/98) |

**Key differences:**
1. **New** was missing: ALL Refurbishment items (11/1-4) and TaxUndercollection (13/98)
2. **Used** was missing: TaxUndercollection (13/98)
3. **Repo** was missing: TaxUndercollection (13/98)
4. **Important distinction:** Cat 11 Item 4 is "Refurbishment/Drapes" (only removed for New), while Cat 15 Item 4 is "Decorating/DecoratingDrapes" (removed for Used and Repo). These are DIFFERENT items despite both being called "Drapes".

---

## Finding 7: Playbook SQL queries used wrong schema assumptions [RESOLVED]

**Severity:** Document error (corrected in updated document)
**Status:** Resolved — all SQL queries in the playbook have been corrected.

Issues found in the original playbook SQL:
1. `packages.packages` does NOT have an `is_deleted` column — only `sales.sales` does
2. There is no single `details` JSONB column — each line type has its own: `home_details`, `land_details`, `tax_details`, `insurance_details`, `warranty_details`, `trade_in_details`, `sales_team_details`, `project_cost_details`, `credit_details`
3. SQL queries checking `should_exclude_from_pricing` are unreliable for Land, TradeIn, Credit, and SalesTeam types (see Finding 3)

---

## Finding 8: Validator requires LandEquity for CustomerLandPayoff

**Severity:** Information (not a bug, but playbook needed adjustment)

**Observed:**
PUT land with `CustomerLandPayoff` type but without `landEquity` field → 400 Bad Request: "LandEquity is required for CustomerLandPayoff."

The `UpdatePackageLandCommandValidator` enforces `LandEquity` as required when `LandInclusion == CustomerLandPayoff`. The playbook request examples have been updated to include this field.

---

## Verification Summary

### Playbook 1 (Read-Only) Results:

| Check | Result | Notes |
|-------|--------|-------|
| GET sale by ID | PASS | Correct response envelope |
| GET packages for sale | PASS | Primary package ranking=1 |
| GET package by ID | PASS | Full snapshot with all lines |
| GP calculation | **FAIL** | GP=0 for all seeded packages (Finding 2) |
| ShouldExcludeFromPricing | **PARTIAL** | API values correct; DB values wrong for 4 types (Finding 3) |
| Responsibility values | **FAIL** | Home/Land seeded as Buyer, should be Seller (Finding 1) |
| SQL structural verification | PASS | All expected line types present, JSONB populated |

### Playbook 2 (Home Mutations) Results:

| Check | Result | Notes |
|-------|--------|-------|
| PUT single-section home | **FAIL** | 500 error from iSeries W&A call (Finding 4) |
| PUT multi-section home | PASS | GP recalculated correctly (96000) |
| Responsibility after PUT | PASS | Home flipped from Buyer to Seller |
| W&A multi-section suppression | PASS | No W&A project costs created for double-wide |
| Home type change pruning | NOT TESTED | No seeded project costs in Cat 11/12/13/15 on test package |
| Tax flag after PUT | PASS | mustRecalculateTaxes=true |

### Playbook 3 (Land Mutations) Results:

| Check | Result | Notes |
|-------|--------|-------|
| CustomerLandPayoff pricing | PASS | salePrice=estimatedCost=payoffAmountFinancing (20000) |
| CustomerLandPayoff Land Payoff | PASS | Cat 2/Item 1 created, excl=true, prices mirror land |
| LandPurchase pricing | PASS | Repriced to purchasePrice |
| Non-priced (HomeOnly) | PASS | salePrice=estimatedCost=0, Land Payoff removed |
| HomeCenterOwnedLand | NOT TESTED | Requires valid LandParcel cache entry for stock lookup |
| Land responsibility after PUT | PASS | Flipped from Buyer to Seller |
| Tax flag after land change | PASS | mustRecalculateTaxes=true |
| JSONB conditional fields | PASS | originalPurchaseDate/Price populated for CustomerLandPayoff |
