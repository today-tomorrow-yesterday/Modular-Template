# TradeInLine — Pricing Guide

**In GP?** No — the TradeInLine itself is excluded. Its economic impact flows
through the auto-generated Trade Over Allowance project cost.

---

## What the user sends

```
PUT /packages/{id}/trade-ins
{
    "items": [
        {
            "salePrice": 15000,          ← stored directly, but excluded from GP
            "estimatedCost": 10000,      ← stored directly, but excluded from GP
            "retailSalePrice": 15000,    ← stored directly, but excluded from GP
            "tradeType": "Single Wide",
            "year": 2015,
            "make": "Clayton",
            "model": "Riverview",
            "tradeAllowance": 20000,     ← THIS drives the GP impact
            "bookInAmount": 15000,       ← THIS drives the GP impact
            "payoffAmount": 10000        ← funding/disclosure only
        }
    ]
}
```

## What happens to the three pricing fields

```
User sends salePrice: 15000       →  TradeInLine.SalePrice = 15000       no transformation
User sends estimatedCost: 10000   →  TradeInLine.EstimatedCost = 10000   no transformation
User sends retailSalePrice: 15000 →  TradeInLine.RetailSalePrice = 15000 no transformation
```

**All three pass straight through** — but none of them matter for GP because
the TradeInLine is excluded from pricing (`ShouldExcludeFromPricing = true`).

## What actually affects GP: TradeAllowance and BookInAmount

The real financial impact comes from two detail fields, not the pricing triple:

```
User sends tradeAllowance: 20000   (what we offered the customer)
User sends bookInAmount: 15000     (what the trade is actually worth)

overAllowance = 20000 - 15000 = 5000  (we're overpaying by $5,000)

IF overAllowance > 0:
  → creates ProjectCostLine (Cat 10, Item 9)
      SalePrice = 0
      EstimatedCost = 5000
      ShouldExcludeFromPricing = false

  GP += 0 - 5000 = -5000   (reduces GP by the over-allowance)
```

If `tradeAllowance <= bookInAmount` (no over-allowance), no project cost is created
and the trade-in has zero GP impact.

---

## Multiple trade-ins

The request accepts an array. Each trade-in gets its own TradeInLine AND its own
Trade Over Allowance project cost (if applicable). All existing trade-ins are
replaced on every PUT (delete-all-then-insert).

```
Item 1: tradeAllowance 20000, bookInAmount 15000 → overAllowance 5000  → GP -= 5000
Item 2: tradeAllowance 5000,  bookInAmount 5000  → overAllowance 0     → no PC created
```

---

## What the API returns

```json
{ "grossProfit": 15000, "commissionableGrossProfit": 0, "mustRecalculateTaxes": true }
```
