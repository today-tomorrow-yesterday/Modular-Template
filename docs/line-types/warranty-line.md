# WarrantyLine — Pricing Guide

**In GP?** Yes — `GP += SalePrice - 0` = the warranty premium

---

## Two ways to set warranty, same pricing pattern

Only **one** field drives pricing: the warranty amount / premium. EstimatedCost
and RetailSalePrice are always zero.

### 1. Admin override (`PUT /packages/{id}/warranty`)

```json
{
    "warrantySelected": true,
    "warrantyAmount": 800          ← THIS becomes SalePrice
}
```

```
User sends warrantyAmount: 800   →  WarrantyLine.SalePrice = 800
                                 →  WarrantyLine.EstimatedCost = 0      (always)
                                 →  WarrantyLine.RetailSalePrice = 0    (always)
```

### 2. iSeries quote (`POST /sales/{id}/warranty/quote`)

```
POST /sales/{id}/warranty/quote
(empty body — no user input)
```

**No user input at all.** The handler derives all inputs from the existing home
line (width, model year, condition, modular type) and delivery address (state, zip).

```
iSeries returns Premium: 750, SalesTaxPremium: 50

→  WarrantyLine.SalePrice = 750         (from iSeries, not user)
→  WarrantyLine.EstimatedCost = 0       (always)
→  WarrantyLine.RetailSalePrice = 0     (always)
```

`SalesTaxPremium` (50) is stored in `WarrantyDetails`, not in the pricing fields.
It's used later by the tax calculation flow.

---

## GP impact

```
GP += 800 - 0 = 800   (full premium flows into GP as revenue)
```

---

## Tax change detection

Both handlers flag taxes for recalculation when the warranty amount changes or
when the warranty is being selected for the first time:

```
IF warrantyAmount changed OR warranty was not previously selected:
  → clear existing tax calculations
  → remove Use Tax project cost
  → MustRecalculateTaxes = true
```

---

## What the API returns

**Admin:**
```json
{ "grossProfit": 20800, "commissionableGrossProfit": 0, "mustRecalculateTaxes": true }
```

**Quote:**
```json
{ "premium": 750, "salesTaxPremium": 50, "warrantySelected": true }
```
