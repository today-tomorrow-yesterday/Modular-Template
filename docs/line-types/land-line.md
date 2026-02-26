# LandLine — Pricing Guide

**In GP?** No — land is excluded from Gross Profit entirely.

---

## What the user sends

```
PUT /packages/{id}/land
{
    "salePrice": 50000,                    ← IGNORED (overwritten by handler)
    "estimatedCost": 40000,                ← IGNORED (overwritten by handler)
    "retailSalePrice": 60000,              ← kept as-is
    "landPurchaseType": "CustomerWantsToPurchaseLand",
    "typeOfLandWanted": "LandPurchase",
    "purchasePrice": 50000,                ← THIS drives SalePrice and EstimatedCost
    ...
}
```

## What happens to the three pricing fields

**SalePrice and EstimatedCost from the request are always overwritten.** The handler
replaces them with values derived from the land detail fields. Only RetailSalePrice
survives from the request.

Which detail fields drive the pricing depends on the land type:

### Branch 1: `landInclusion = "CustomerLandPayoff"`
Customer is paying off their existing land mortgage as part of the deal.

```
User sends payoffAmountFinancing: 75000

  → LandLine.SalePrice     = 75000    (from payoffAmountFinancing)
  → LandLine.EstimatedCost = 75000    (same — no dealer margin)
  → LandLine.RetailSalePrice = 60000  (from request, unchanged)
```

### Branch 2: `typeOfLandWanted = "LandPurchase"`
Customer is buying a third-party land parcel.

```
User sends purchasePrice: 50000

  → LandLine.SalePrice     = 50000    (from purchasePrice)
  → LandLine.EstimatedCost = 50000    (same — no dealer margin)
  → LandLine.RetailSalePrice = 60000  (from request, unchanged)
```

### Branch 3: `typeOfLandWanted = "HomeCenterOwnedLand"`
Customer is buying land owned by the dealership. This is the only branch with a
dealer margin (SalePrice != EstimatedCost).

```
User sends landSalesPrice: 80000, landCost: 60000

  → LandLine.SalePrice     = 80000    (from landSalesPrice — what customer pays)
  → LandLine.EstimatedCost = 60000    (from landCost — what dealer paid)
  → LandLine.RetailSalePrice = 60000  (from request, unchanged)
```

### Branch 4: Everything else (CustomerOwned, Community, PrivateProperty)
No land pricing impact.

```
  → LandLine.SalePrice     = 0
  → LandLine.EstimatedCost = 0
  → LandLine.RetailSalePrice = 60000  (from request, unchanged)
```

### Priority
Branch 1 (`CustomerLandPayoff`) is checked first — it's on `landInclusion`, not
`typeOfLandWanted`. If it matches, branches 2-3 are skipped even if `typeOfLandWanted`
is also set.

---

## GP impact

```
GP += 0   (LandLine is excluded from pricing regardless of branch)
```

Even in Branch 3 where there's a $20,000 dealer margin, it does NOT enter GP.
This matches legacy behavior — land was never part of the GP formula.

---

## Side effect: Land Payoff shadow project cost

When the land has a positive SalePrice (branches 1, 2, or 3), the handler creates
a shadow ProjectCostLine that mirrors the land pricing. This shadow is also excluded
from GP — it exists for commission and funding calculations.

```
LandLine.SalePrice = 50000          →  ProjectCostLine (Cat 2, Item 1)
LandLine.EstimatedCost = 50000           SalePrice = 50000
                                         EstimatedCost = 50000
                                         ShouldExcludeFromPricing = true
                                         GP += 0  (excluded)
```

---

## What the API returns

```json
{ "grossProfit": 20000, "commissionableGrossProfit": 0, "mustRecalculateTaxes": true }
```

`mustRecalculateTaxes` is `true` only if the land's SalePrice actually changed
from what was previously stored.
