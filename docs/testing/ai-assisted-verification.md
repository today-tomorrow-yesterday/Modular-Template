# AI-Assisted Integration Verification

## 1. Overview & Philosophy

This document provides **executable playbooks** for AI-driven exploratory verification of the Sales API. It is NOT traditional xUnit integration testing. Instead, Claude Code acts as an AI QA engineer — calling the local API, querying the database, and comparing results against documented business rules.

**How it works:** Claude reads the handler code + these docs, calls the API, queries the DB, and verifies invariants. Each playbook is a sequence of steps that can be invoked by saying _"run playbook X."_

**When to use it:**
- After handler changes (home, land, project cost, etc.)
- After seeder updates or schema migrations
- Before merging feature branches to main
- When onboarding — to build confidence that the system works as documented

**Source of truth hierarchy** (most authoritative first):
1. **The actual handler code** in this repo — this IS the system
2. **The legacy Sales API** — the original implementation the new system must match
3. **Target journey docs** at `X:\SES\Legacy\docs\...\Target\` — ~80-90% accurate, useful as reference but NOT the final word

**Cross-reference:** Target journey docs provide business context for each handler. They were derived from the legacy Sales API. When in doubt, read the handler code.

---

## 2. Prerequisites & Setup

### Start the API

```bash
cd X:\SES\223-MM-template\Modular-Template
dotnet run --project src/Api/Host.Sales --launch-profile https
```

The API starts at `https://localhost:5104` (HTTPS) or `http://localhost:5004` (HTTP).

### Ensure DB is seeded

In `appsettings.Development.json`, confirm:

```json
"Seeding": {
  "Enabled": true,
  "Seed": 12345
}
```

The seed value `12345` produces deterministic data via Bogus — GUIDs are stable across re-seeds.

### Variables used in playbooks

| Variable | Default | Description |
|----------|---------|-------------|
| `${BASE_URL}` | `http://localhost:5004/api/v1` | API base URL (adjust to match your config) |
| `${SALE_ID}` | Discovered in Step 0 | Sale PublicId from seeded data |
| `${PACKAGE_ID}` | Discovered in Step 0 | Package PublicId from seeded data |

### Database access

```bash
psql -h localhost -d sales_dev -U postgres
```

### Warnings

**Encryption:** Pricing columns (`sale_price`, `estimated_cost`, `retail_sale_price`, `gross_profit`, `commissionable_gross_profit`) are encrypted with AES-256-GCM via the `[SensitiveData]` attribute. SQL queries on these columns return ciphertext. **All price verification must go through the API.**

**iSeries stub:** The iSeries integration points to `http://localhost:9999` in dev. Calls to W&A pricing, tax calculation, insurance quotes, warranty quotes, and commission calculation will fail with connection refused. Playbook steps that depend on iSeries are marked with ⚡. **Single-section home updates with a W&A option will 500 — use multi-section homes or null W&A option in dev.**

**DB column caveat:** The `should_exclude_from_pricing` column is **unreliable** for Land, TradeIn, Credit, and SalesTeam line types. These types use C# property overrides that return `true` regardless of the DB value (which stores `false`). Always verify exclusion flags via the API, not SQL. See `docs/audits/verification-audit-2026-02-27.md` Finding 3.

**Seeded GP = 0:** All seeded packages have `grossProfit = 0` because the seeder bypasses the `Package.AddLine()` aggregate method. GP is correctly calculated on the first handler invocation. See audit Finding 2.

---

## 3. Step 0 — Discover Seed Data

Run these bootstrap queries to find the actual PublicIds from seeded data. Every playbook begins here.

```sql
-- Find all sales with their packages
-- NOTE: packages table does NOT have is_deleted; only sales does
SELECT s.public_id AS sale_id, p.public_id AS package_id, p.name, p.ranking, p.must_recalculate_taxes
FROM sales.sales s
JOIN packages.packages p ON p.sale_id = s.id
WHERE s.is_deleted = false
ORDER BY s.id, p.ranking DESC;
```

```sql
-- Find package line structure for a specific package
-- NOTE: Each line type has its own JSONB column (home_details, land_details, etc.)
-- NOTE: should_exclude_from_pricing is unreliable in SQL for Land/TradeIn/Credit/SalesTeam
SELECT pl.line_type, pl.responsibility, pl.should_exclude_from_pricing, pl.sort_order,
       pl.on_lot_home_id, pl.land_parcel_id
FROM packages.package_lines pl
JOIN packages.packages p ON pl.package_id = p.id
WHERE p.public_id = '${PACKAGE_ID}'
ORDER BY pl.line_type, pl.sort_order;
```

Record the `sale_id` and `package_id` values — they are used as `${SALE_ID}` and `${PACKAGE_ID}` in all subsequent steps.

---

## 4. Playbook 1 — Full Package Lifecycle Verification

**Goal:** Verify seeded package state is internally consistent.
**Type:** Read-only — no mutations.

### Step 1: Discover seed data

Run Step 0 queries. Pick a sale with at least one package. Record `${SALE_ID}` and `${PACKAGE_ID}`.

### Step 2: GET sale

```bash
curl -k ${BASE_URL}/sales/${SALE_ID}
```

**Verify:**
- Response is a valid JSON envelope
- Sale has expected fields (`publicId`, `status`, `buyerName`, etc.)
- `publicId` matches `${SALE_ID}`

### Step 3: GET packages for sale

```bash
curl -k ${BASE_URL}/sales/${SALE_ID}/packages
```

**Verify:**
- Package list is non-empty
- At least one package has `ranking: 1` (primary package)
- Each package has a `publicId`, `name`, and line count

### Step 4: GET package by ID

```bash
curl -k ${BASE_URL}/packages/${PACKAGE_ID}
```

**Verify:**
- Response includes full package snapshot with all package lines
- Each line has `lineType`, `salePrice`, `estimatedCost`, `shouldExcludeFromPricing`

### Step 5: Verify GP calculation

From the API response in Step 4:

```
GrossProfit = SUM(salePrice - estimatedCost) WHERE shouldExcludeFromPricing = false
```

Manually sum `(salePrice - estimatedCost)` for all lines where `shouldExcludeFromPricing = false`. Compare to the `grossProfit` field on the package. They must match.

### Step 6: Verify shouldExcludeFromPricing flags

Check each line type against the expected values:

| Line Type | Expected (API) | DB Column | Notes |
|-----------|----------------|-----------|-------|
| Home | `false` | `false` | Contributes to GP |
| Land | `true` | `false` (stale!) | Domain override — land never in GP; pricing flows through Land Payoff |
| Tax | `true` | `true` | Excluded — taxes are a pass-through |
| Insurance | `true` | `true` | Excluded — insurance premium |
| Warranty | `false` | `false` | Contributes to GP |
| Trade In | `true` | `false` (stale!) | Domain override — trade-in is a credit, not price |
| Project Cost | `false` | `false` | Default; except Land Payoff (Cat 2, Item 1) which is `true` |
| Credit (DownPayment) | `true` | `false` (stale!) | Domain override — credits reduce total, not price |
| Credit (Concession) | `true` | `false` (stale!) | Domain override |
| Sales Team | `true` | `false` (stale!) | Domain override — metadata-only, all-zero prices |

### Step 7: Verify Responsibility values

| Line Type | Expected (Handler) | Seeded As | Notes |
|-----------|-------------------|-----------|-------|
| Home | `Seller` | `Buyer` (bug) | Handler sets Seller; seeder incorrectly sets Buyer |
| Land | `Seller` | `Buyer` (bug) | Handler sets Seller; seeder incorrectly sets Buyer |
| TradeIn | `Seller` | `Seller` | Correct in both |
| ProjectCost (user) | `Buyer` | `Buyer` | Buyer cost by default |
| ProjectCost (auto) | `Seller` | N/A | W&A, Land Payoff — handler-generated with Seller |
| Warranty | `Seller` | `Seller` | Domain factory hardcodes Seller |
| Credit (DownPayment) | `Buyer` | `Buyer` | Domain factory sets Buyer |
| Credit (Concession) | `Seller` | `Seller` | Domain factory sets Seller |

### Step 8: SQL structural verification

```sql
-- All expected line types present
SELECT pl.line_type, COUNT(*) AS cnt
FROM packages.package_lines pl
JOIN packages.packages p ON pl.package_id = p.id
WHERE p.public_id = '${PACKAGE_ID}'
GROUP BY pl.line_type
ORDER BY pl.line_type;
```

**Verify:**
- All expected line types are present for the package
- Type-specific JSONB details columns are non-null for each line type
- `on_lot_home_id` is populated for OnLot homes, null otherwise
- `land_parcel_id` is populated for HomeCenterOwnedLand, null otherwise
- Primary package has `ranking = 1`

```sql
-- Verify type-specific JSONB details non-null
-- NOTE: Each line type has its own JSONB column (not a shared "details" column)
SELECT pl.line_type, pl.sort_order,
       pl.on_lot_home_id,
       pl.land_parcel_id,
       (pl.home_details IS NOT NULL) AS has_home,
       (pl.land_details IS NOT NULL) AS has_land,
       (pl.tax_details IS NOT NULL) AS has_tax,
       (pl.insurance_details IS NOT NULL) AS has_insurance,
       (pl.warranty_details IS NOT NULL) AS has_warranty,
       (pl.project_cost_details IS NOT NULL) AS has_pc,
       (pl.trade_in_details IS NOT NULL) AS has_trade,
       (pl.sales_team_details IS NOT NULL) AS has_team,
       (pl.credit_details IS NOT NULL) AS has_credit
FROM packages.package_lines pl
JOIN packages.packages p ON pl.package_id = p.id
WHERE p.public_id = '${PACKAGE_ID}'
ORDER BY pl.line_type, pl.sort_order;
```

---

## 5. Playbook 2 — Home Update Cascades

**Goal:** Verify the 8-step home handler cascade works correctly.
**Reference:** `Target/01-HOME-JOURNEY.md`
**Type:** Mutating — sends PUT requests that change package state.

### Step 1: Snapshot current state

```bash
curl -k ${BASE_URL}/packages/${PACKAGE_ID}
```

Record existing lines and GP. This is your baseline.

### Step 2: PUT single-wide home with W&A

Send a home update with `numberOfFloorSections: 1` and `wheelAndAxlesOption: 0` (Rent).

```bash
curl -k -X PUT ${BASE_URL}/packages/${PACKAGE_ID}/home \
  -H "Content-Type: application/json" \
  -d '{
    "homeType": 0,
    "homeSourceType": 0,
    "numberOfFloorSections": 1,
    "wheelAndAxlesOption": 0,
    "stockNumber": "EXISTING-STOCK",
    "salePrice": 85000,
    "estimatedCost": 60000,
    "retailSalePrice": 90000
  }'
```

> ⚡ **iSeries dependency:** W&A pricing call will fail (`localhost:9999`). W&A project cost will NOT be created. This is expected in dev.

**Verify:**
- Home line persisted with `responsibility: Seller`
- GP recalculated (re-run Step 5 from Playbook 1)
- Response shows updated home line values

### Step 3: PUT double-wide home

Same home but with `numberOfFloorSections: 2`:

```bash
curl -k -X PUT ${BASE_URL}/packages/${PACKAGE_ID}/home \
  -H "Content-Type: application/json" \
  -d '{
    "homeType": 0,
    "homeSourceType": 0,
    "numberOfFloorSections": 2,
    "wheelAndAxlesOption": 0,
    "stockNumber": "EXISTING-STOCK",
    "salePrice": 130000,
    "estimatedCost": 95000,
    "retailSalePrice": 140000
  }'
```

**Verify:**
- W&A project cost is NOT created (multi-section suppression — handler line 278-282)
- `wheelAndAxlesOption` is still stored in JSONB details (handler stores it, just doesn't create project cost)
- GP recalculated with new home prices

### Step 4: PUT home type change — New to Used

Change `homeType` from `0` (New) to `1` (Used):

```bash
curl -k -X PUT ${BASE_URL}/packages/${PACKAGE_ID}/home \
  -H "Content-Type: application/json" \
  -d '{
    "homeType": 1,
    "homeSourceType": 0,
    "numberOfFloorSections": 2,
    "wheelAndAxlesOption": 0,
    "stockNumber": "USED-STOCK",
    "salePrice": 75000,
    "estimatedCost": 50000,
    "retailSalePrice": 80000
  }'
```

**Verify:**
- Project cost pruning: Decorating Drapes (Cat 15 / Item 4) and Repo Costs (Cat 12 / *) removed
- `must_recalculate_taxes = true` (home type is tax-affecting)
- GP recalculated

### Step 5: PUT home type change — Used to Repo

Change `homeType` to `2` (Repo):

```bash
curl -k -X PUT ${BASE_URL}/packages/${PACKAGE_ID}/home \
  -H "Content-Type: application/json" \
  -d '{
    "homeType": 2,
    "homeSourceType": 0,
    "numberOfFloorSections": 2,
    "wheelAndAxlesOption": 0,
    "stockNumber": "REPO-STOCK",
    "salePrice": 45000,
    "estimatedCost": 30000,
    "retailSalePrice": 50000
  }'
```

**Verify:**
- Refurb Cleaning (Cat 11 / Item 1), RepairRefurb (Cat 11 / Item 2), RefurbParts (Cat 11 / Item 3) removed
- Decorating Drapes (Cat 15 / Item 4) removed
- Repo Costs (Cat 12 / *) KEPT (this is a repo home)
- GP recalculated

### Step 6: SQL verification

```sql
-- Project cost audit after home type changes
SELECT pl.project_cost_details->>'categoryId' AS cat,
       pl.project_cost_details->>'itemId' AS item,
       pl.project_cost_details->>'itemDescription' AS description,
       pl.should_exclude_from_pricing
FROM packages.package_lines pl
JOIN packages.packages p ON pl.package_id = p.id
WHERE p.public_id = '${PACKAGE_ID}'
  AND pl.line_type = 'Project Cost'
ORDER BY (pl.project_cost_details->>'categoryId')::int, (pl.project_cost_details->>'itemId')::int;
```

**Verify:**
- Auto-generated project costs (W&A, Land Payoff, Use Tax) preserved through home type changes
- User-managed project costs pruned according to the home type matrix

### Step 7: Verify tax flag

After each PUT above, check `must_recalculate_taxes` in both the API response AND the DB:

```sql
SELECT p.public_id, p.must_recalculate_taxes
FROM packages.packages p
WHERE p.public_id = '${PACKAGE_ID}';
```

---

## 6. Playbook 3 — Land Repricing Matrix

**Goal:** Verify all 4 land pricing branches and Land Payoff project cost lifecycle.
**Reference:** `Target/02-LAND-JOURNEY.md`
**Type:** Mutating — sends PUT requests that change package state.

### Step 1: CustomerLandPayoff

PUT land with `CustomerHasLand` / `CustomerOwnedLand` / `CustomerLandPayoff`:

```bash
curl -k -X PUT ${BASE_URL}/packages/${PACKAGE_ID}/land \
  -H "Content-Type: application/json" \
  -d '{
    "landPurchaseType": "CustomerHasLand",
    "customerLandType": "CustomerOwnedLand",
    "landInclusion": "CustomerLandPayoff",
    "payoffAmountFinancing": 20000,
    "landEquity": 15000,
    "salePrice": 20000,
    "estimatedCost": 20000,
    "retailSalePrice": 40000,
    "estimatedValue": 45000,
    "sizeInAcres": 5,
    "propertyOwner": "Test Owner",
    "financedBy": "Local Credit Union",
    "originalPurchaseDate": "2017-12-03",
    "originalPurchasePrice": 15000,
    "propertyOwnerPhoneNumber": "5551234567"
  }'
```

**Verify:**
- Response shows `salePrice = 20000`, `estimatedCost = 20000` (both repriced to `payoffAmountFinancing`)
- Land Payoff project cost (Cat 2, Item 1) created with matching prices
- Land Payoff has `shouldExcludeFromPricing = true`

```sql
-- Verify Land JSONB for CustomerLandPayoff
SELECT pl.land_details->>'originalPurchaseDate' AS orig_date,
       pl.land_details->>'originalPurchasePrice' AS orig_price
FROM packages.package_lines pl
JOIN packages.packages p ON pl.package_id = p.id
WHERE p.public_id = '${PACKAGE_ID}'
  AND pl.line_type = 'Land';
```

**Verify:** `originalPurchaseDate` and `originalPurchasePrice` are populated.

### Step 2: LandPurchase

PUT land with `CustomerWantsToPurchaseLand` / `LandPurchase`:

```bash
curl -k -X PUT ${BASE_URL}/packages/${PACKAGE_ID}/land \
  -H "Content-Type: application/json" \
  -d '{
    "landPurchaseType": "CustomerWantsToPurchaseLand",
    "typeOfLandWanted": "LandPurchase",
    "purchasePrice": 50000,
    "salePrice": 99999,
    "estimatedCost": 99999
  }'
```

**Verify:**
- Response shows `salePrice = 50000`, `estimatedCost = 50000` (both repriced to `purchasePrice`)
- Land Payoff project cost (Cat 2, Item 1) created with matching prices

### Step 3: HomeCenterOwnedLand

PUT land with `CustomerWantsToPurchaseLand` / `HomeCenterOwnedLand`:

```bash
curl -k -X PUT ${BASE_URL}/packages/${PACKAGE_ID}/land \
  -H "Content-Type: application/json" \
  -d '{
    "landPurchaseType": "CustomerWantsToPurchaseLand",
    "typeOfLandWanted": "HomeCenterOwnedLand",
    "landStockNumber": "${STOCK}",
    "landSalesPrice": 75000,
    "landCost": 50000,
    "salePrice": 99999,
    "estimatedCost": 99999
  }'
```

**Verify:**
- Response shows `salePrice = 75000` (from `landSalesPrice`), `estimatedCost = 50000` (from `landCost`) — this is the dealer margin
- Land Payoff project cost mirrors these prices

### Step 4: Non-priced type — PrivateProperty / HomeOnly

PUT land with `CustomerHasLand` / `PrivateProperty` / `HomeOnly`:

```bash
curl -k -X PUT ${BASE_URL}/packages/${PACKAGE_ID}/land \
  -H "Content-Type: application/json" \
  -d '{
    "landPurchaseType": "CustomerHasLand",
    "customerLandType": "PrivateProperty",
    "landInclusion": "HomeOnly",
    "salePrice": 99999,
    "estimatedCost": 99999
  }'
```

**Verify:**
- Response shows `salePrice = 0`, `estimatedCost = 0` (repriced to zero)
- Land Payoff project cost REMOVED (non-priced land type)

### Step 5: Tax flag verification

After each PUT above, verify `must_recalculate_taxes` changes when land price changes:

```sql
SELECT p.public_id, p.must_recalculate_taxes
FROM packages.packages p
WHERE p.public_id = '${PACKAGE_ID}';
```

### Step 6: JSONB conditional fields

```sql
-- Verify conditional fields are populated only for relevant land types
SELECT pl.land_details->>'landPurchaseType' AS purchase_type,
       pl.land_details->>'customerLandType' AS customer_type,
       pl.land_details->>'landInclusion' AS inclusion,
       pl.land_details->>'typeOfLandWanted' AS type_wanted,
       pl.land_details->>'communityNumber' AS community_num,
       pl.land_details->>'communityName' AS community_name,
       pl.land_details->>'originalPurchaseDate' AS orig_date,
       pl.land_details->>'originalPurchasePrice' AS orig_price,
       pl.land_details->>'landStockNumber' AS stock_num
FROM packages.package_lines pl
JOIN packages.packages p ON pl.package_id = p.id
WHERE p.public_id = '${PACKAGE_ID}'
  AND pl.line_type = 'Land';
```

**Verify:**
- Community fields (`communityNumber`, `communityName`, etc.) only populated for `CommunityOrNeighborhood`
- `originalPurchaseDate` / `originalPurchasePrice` only for `CustomerLandPayoff`
- `landStockNumber` / `landCost` / `landSalesPrice` only for `HomeCenterOwnedLand`

---

## 7. Business Rules Checklist

Quick-reference table of ALL invariants that playbooks verify.

### GP Formula

```
GrossProfit = SUM(SalePrice - EstimatedCost) WHERE NOT ShouldExcludeFromPricing
```

### CommissionableGP

Only updated by `POST /packages/{id}/commission` (⚡ iSeries). Initially equals GP.

### MustRecalculateTaxes Triggers

The `must_recalculate_taxes` flag is set to `true` when any of these 9 conditions occur:

| # | Trigger | Handler |
|---|---------|---------|
| 1 | HomeType changed | Home |
| 2 | Home StockNumber changed | Home |
| 3 | Home SalePrice changed | Home |
| 4 | Land SalePrice changed | Land |
| 5 | ProjectCost count changed (WHERE NOT ShouldExcludeFromPricing) | ProjectCost |
| 6 | ProjectCost SalePrice changed | ProjectCost |
| 7 | WarrantyAmount changed | Warranty |
| 8 | WarrantySelected changed | Warranty |
| 9 | TradeIn SalePrice changed | TradeIn |

### Auto-Generated Project Costs

These 6 well-known project costs are auto-generated by handlers (never created manually):

| Name | Cat | Item | Owner | ExcludeFromPricing |
|------|-----|------|-------|--------------------|
| W&A Rental | 1 | 28 | Home handler | `false` |
| W&A Purchase | 1 | 29 | Home handler | `false` |
| Land Payoff | 2 | 1 | Land handler | `true` |
| Use Tax | 9 | 21 | Tax calculation | `false` |
| Trade Over Allowance | 10 | 9 | Trade-In handler | `false` |
| Seller Paid Closing Cost | 14 | 1 | Concessions handler | `false` |

### ShouldExcludeFromPricing by Line Type

**Use API responses for verification — the DB column is stale for types marked (override).**

| Line Type | Excluded? | Mechanism | DB Accurate? | Reason |
|-----------|-----------|-----------|-------------|--------|
| Home | No | Override → `false` | Yes | Core sale item |
| Land | Yes | Override → `true` | **No** (shows `false`) | Legacy never included land in GP |
| Tax | Yes | Explicit `true` | Yes | Pass-through |
| Insurance | Yes | Explicit `true` | Yes | Premium, not sale revenue |
| Warranty | No | Configurable | Yes | Contributes to GP |
| TradeIn | Yes | Override → `true` | **No** (shows `false`) | Credit, not price component |
| ProjectCost | No (default) | Explicit | Yes | Except Land Payoff (Cat 2/1) = `true` |
| Credit | Yes | Override → `true` | **No** (shows `false`) | Reduce total, not price |
| SalesTeam | Yes | Override → `true` | **No** (shows `false`) | Metadata-only, all-zero prices |

### Occupancy Eligibility

**Eligible:** Primary Residence, Secondary Residence, Buy for Immediate Family, Buy for Other
**Ineligible:** Rental, Investment

When occupancy becomes **ineligible**: Insurance and Warranty lines are removed from all draft packages.

### Domain Event Cascades from DeliveryAddress

| Change | Effect |
|--------|--------|
| State changed | Clear tax question answers, set `MustRecalculateTaxes = true` |
| Occupancy became ineligible | Remove Insurance + Warranty from all draft packages |
| Location changed (city/state/zip/county/isWithinCityLimits) | Clear TaxItems, remove Use Tax project cost, set `MustRecalculateTaxes = true` |

### Home Type → Project Cost Pruning Matrix

When home type changes, certain project costs are removed. Source: `UpdatePackageHomeCommandHandler.cs:231-253`.

Note: W&A project costs (Cat 1/28, Cat 1/29) are always removed and recalculated in Step 5 regardless of home type.

| Home Type | Removes |
|-----------|---------|
| New | Refurb/Cleaning (11/1), Refurb/RepairRefurb (11/2), Refurb/RefurbParts (11/3), Refurb/Drapes (11/4), ALL RepoCosts (Cat 12/*), MiscTax/TaxUndercollection (13/98) |
| Used | ALL RepoCosts (Cat 12/*), Decorating/DecoratingDrapes (15/4), MiscTax/TaxUndercollection (13/98) |
| Repo | Refurb/Cleaning (11/1), Refurb/RepairRefurb (11/2), Refurb/RefurbParts (11/3), Decorating/DecoratingDrapes (15/4), MiscTax/TaxUndercollection (13/98) |

**Important:** Cat 11 Item 4 ("Refurbishment/Drapes") and Cat 15 Item 4 ("Decorating/DecoratingDrapes") are different items. New removes Cat 11/4; Used and Repo remove Cat 15/4.

---

## 8. Database Verification Queries

Pre-written SQL for structural verification. All queries target non-encrypted metadata fields.

### Query 1: Full package line inventory

```sql
-- NOTE: should_exclude_from_pricing is stale for Land/TradeIn/Credit/SalesTeam (see audit)
SELECT pl.line_type, pl.responsibility, pl.should_exclude_from_pricing,
       pl.sort_order, pl.on_lot_home_id, pl.land_parcel_id
FROM packages.package_lines pl
JOIN packages.packages p ON pl.package_id = p.id
WHERE p.public_id = '${PACKAGE_ID}'
ORDER BY pl.line_type, pl.sort_order;
```

### Query 2: Auto-generated project cost audit

```sql
SELECT pl.project_cost_details->>'categoryId' AS cat,
       pl.project_cost_details->>'itemId' AS item,
       pl.project_cost_details->>'itemDescription' AS description,
       pl.should_exclude_from_pricing
FROM packages.package_lines pl
JOIN packages.packages p ON pl.package_id = p.id
WHERE p.public_id = '${PACKAGE_ID}'
  AND pl.line_type = 'Project Cost'
  AND (pl.project_cost_details->>'categoryId')::int IN (1, 2, 9, 10, 14)
ORDER BY (pl.project_cost_details->>'categoryId')::int, (pl.project_cost_details->>'itemId')::int;
```

### Query 3: Tax flag status across all packages

```sql
SELECT p.public_id, p.name, p.ranking, p.must_recalculate_taxes
FROM packages.packages p
JOIN sales.sales s ON p.sale_id = s.id
WHERE s.is_deleted = false
ORDER BY s.id, p.ranking DESC;
```

### Query 4: Land JSONB conditional field verification

```sql
SELECT pl.land_details->>'landPurchaseType' AS purchase_type,
       pl.land_details->>'customerLandType' AS customer_type,
       pl.land_details->>'landInclusion' AS inclusion,
       pl.land_details->>'typeOfLandWanted' AS type_wanted,
       pl.land_details->>'communityNumber' AS community_num,
       pl.land_details->>'communityName' AS community_name,
       pl.land_details->>'originalPurchaseDate' AS orig_date,
       pl.land_details->>'originalPurchasePrice' AS orig_price,
       pl.land_details->>'landStockNumber' AS stock_num
FROM packages.package_lines pl
JOIN packages.packages p ON pl.package_id = p.id
WHERE p.public_id = '${PACKAGE_ID}'
  AND pl.line_type = 'Land';
```

### Query 5: Home JSONB key fields

```sql
SELECT pl.home_details->>'homeType' AS home_type,
       pl.home_details->>'homeSourceType' AS source_type,
       pl.home_details->>'numberOfFloorSections' AS floor_sections,
       pl.home_details->>'wheelAndAxlesOption' AS wa_option,
       pl.home_details->>'stockNumber' AS stock_num,
       pl.on_lot_home_id
FROM packages.package_lines pl
JOIN packages.packages p ON pl.package_id = p.id
WHERE p.public_id = '${PACKAGE_ID}'
  AND pl.line_type = 'Home';
```

### Query 6: ShouldExcludeFromPricing audit

```sql
-- CAVEAT: DB values are WRONG for Land, TradeIn, Credit, SalesTeam (always false, should be true)
-- Use this query to audit Tax/Insurance/Warranty/ProjectCost/Home only
SELECT pl.line_type, pl.should_exclude_from_pricing, COUNT(*) AS cnt
FROM packages.package_lines pl
JOIN packages.packages p ON pl.package_id = p.id
WHERE p.public_id = '${PACKAGE_ID}'
GROUP BY pl.line_type, pl.should_exclude_from_pricing
ORDER BY pl.line_type;
```
