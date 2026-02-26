# InsuranceLine — Pricing Guide

**In GP?** Yes — `GP += SalePrice - 0` = the insurance premium

---

## Three ways to set insurance, same pricing pattern

Only **one** user-provided field affects pricing: the premium amount. EstimatedCost
and RetailSalePrice are always zero — insurance has no dealer cost and no retail ceiling.

### 1. Admin override (`PUT /packages/{id}/insurance`)

```json
{
    "totalPremium": 1200,         ← THIS becomes SalePrice
    "insuranceType": "HomeFirst",
    "coverageAmount": 150000,
    "companyName": "HomeFirst",
    ... (5 other config fields)
}
```

```
User sends totalPremium: 1200     →  InsuranceLine.SalePrice = 1200
                                  →  InsuranceLine.EstimatedCost = 0      (always)
                                  →  InsuranceLine.RetailSalePrice = 0    (always)
```

### 2. HomeFirst quote (`POST /sales/{id}/insurance/home-first-quote`)

```json
{
    "coverageAmount": 150000,
    "occupancyType": "O",
    "customerBirthDate": "1985-03-15",
    ... (8 other config fields — no pricing fields)
}
```

**No premium in the request.** iSeries calculates it from coverage + demographics:

```
iSeries returns TotalPremium: 1500

→  InsuranceLine.SalePrice = 1500       (from iSeries, not user)
→  InsuranceLine.EstimatedCost = 0      (always)
→  InsuranceLine.RetailSalePrice = 0    (always)
```

### 3. Outside insurance (`POST /sales/{id}/insurance/outside`)

```json
{
    "providerName": "State Farm",
    "coverageAmount": 200000,
    "premiumAmount": 1000          ← THIS becomes SalePrice
}
```

```
User sends premiumAmount: 1000    →  InsuranceLine.SalePrice = 1000
                                  →  InsuranceLine.EstimatedCost = 0      (always)
                                  →  InsuranceLine.RetailSalePrice = 0    (always)
```

---

## GP impact

```
GP += 1200 - 0 = 1200   (full premium flows into GP as revenue)
```

---

## No side effects

No auto-generated project costs. No tax change detection.

Note: When the home changes, the `HomeLineUpdatedDomainEvent` handler strips the
HomeFirst InsuranceLine automatically (stale quote). That's a Home handler side
effect, not an Insurance handler side effect.

---

## What the API returns

**Admin/Outside:** `{ "grossProfit": ..., "mustRecalculateTaxes": ... }`

**HomeFirst quote:**
```json
{ "premium": 1500, "coverageAmount": 150000, "maxCoverage": 300000, "isEligible": true }
```
