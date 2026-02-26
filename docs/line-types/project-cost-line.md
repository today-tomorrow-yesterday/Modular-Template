# ProjectCostLine — Pricing Guide

**In GP?** Depends — the user controls the `shouldExcludeFromPricing` flag per line.

---

## What the user sends (user-managed project costs)

```
PUT /packages/{id}/project-costs
{
    "items": [
        {
            "categoryId": 5,
            "itemId": 3,
            "salePrice": 3000,                  ← stored directly
            "estimatedCost": 2000,              ← stored directly
            "retailSalePrice": 3000,            ← stored directly
            "shouldExcludeFromPricing": false,   ← user controls GP participation
            "responsibility": "Seller"
        }
    ]
}
```

## What happens to the three pricing fields

```
User sends salePrice: 3000       →  ProjectCostLine.SalePrice = 3000           no transformation
User sends estimatedCost: 2000   →  ProjectCostLine.EstimatedCost = 2000       no transformation
User sends retailSalePrice: 3000 →  ProjectCostLine.RetailSalePrice = 3000     no transformation
User sends shouldExclude: false  →  ProjectCostLine.ShouldExcludeFromPricing = false
```

**All three pass straight through.** This is the only line type where the user
also controls the `shouldExcludeFromPricing` flag.

```
GP += 3000 - 2000 = 1000   (when shouldExcludeFromPricing = false)
GP += 0                     (when shouldExcludeFromPricing = true)
```

---

## Auto-generated project costs (user CANNOT edit these)

Six project cost keys are system-managed. The handler rejects user requests
containing these keys. They are created/updated by other handlers as side effects:

| Name | Cat/Item | Who sets the pricing | User field that drives it |
|------|----------|---------------------|--------------------------|
| W&A Rental | 1/28 | Home handler → iSeries | `wheelAndAxlesOption`, `stockNumber` or wheel/axle counts |
| W&A Purchase | 1/29 | Home handler → iSeries | same |
| Land Payoff | 2/1 | Land handler | `purchasePrice`, `payoffAmountFinancing`, `landSalesPrice`/`landCost` |
| Use Tax | 9/21 | Tax handler → iSeries | (no direct user input — computed by iSeries) |
| Trade Over Allowance | 10/9 | Trade-in handler | `tradeAllowance` - `bookInAmount` |
| Seller Paid Closing Cost | 14/1 | Concession handler | concession `amount` |

See each line type's doc for details on how user inputs flow into these.

---

## What the API returns

```json
{ "grossProfit": 21000, "commissionableGrossProfit": 0, "mustRecalculateTaxes": true }
```

`mustRecalculateTaxes` is `true` if the count or prices of non-excluded project
costs changed from what was previously stored.
