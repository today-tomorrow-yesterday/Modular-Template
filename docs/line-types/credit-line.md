# CreditLine — Pricing Guide

**In GP?** No — credits are excluded. But Concessions reduce GP indirectly
through an auto-generated project cost.

Two subtypes: **DownPayment** and **Concession**.

---

## DownPayment (`PUT /packages/{id}/down-payment`)

### What the user sends

```json
{ "amount": 5000 }
```

One field. That's it.

### What happens to the three pricing fields

```
User sends amount: 5000          →  CreditLine.SalePrice = 5000    (the credit amount)
                                 →  CreditLine.EstimatedCost = 0   (always)
                                 →  CreditLine.RetailSalePrice = 0 (always)
```

### GP impact

```
GP += 0   (excluded from pricing)
```

Down payment has zero effect on GP. It reduces the amount financed but does not
change the dealer's margin.

### Side effects

None. No tax change. No domain events. No auto-generated project costs.

---

## Concession (`PUT /packages/{id}/concessions`)

### What the user sends

```json
{ "amount": 3000 }
```

One field. Same as down payment.

### What happens to the three pricing fields

```
User sends amount: 3000          →  CreditLine.SalePrice = 3000    (the concession amount)
                                 →  CreditLine.EstimatedCost = 0   (always)
                                 →  CreditLine.RetailSalePrice = 0 (always)
```

### GP impact (indirect)

The CreditLine itself is excluded from GP. But the handler auto-generates a
**Seller Paid Closing Cost** project cost that carries the real economic impact:

```
User sends amount: 3000

→  CreditLine (excluded from GP)
     SalePrice = 3000, EstimatedCost = 0
     GP += 0

→  ProjectCostLine (Cat 14, Item 1) — auto-generated
     SalePrice = 0
     EstimatedCost = 3000              ← mirrors the concession amount
     ShouldExcludeFromPricing = false

     GP += 0 - 3000 = -3000           ← reduces GP
```

The concession is a seller cost — the dealer absorbs it. The GP reduction shows
that the dealer's margin shrinks by the concession amount.

If `amount = 0`, both the CreditLine and the Seller Paid Closing Cost PC are removed.

---

## What the API returns

Both subtypes:
```json
{ "grossProfit": 14000, "commissionableGrossProfit": 0, "mustRecalculateTaxes": false }
```

For concessions, `mustRecalculateTaxes` may be `true` if the Seller Paid Closing Cost
PC was added or removed (changes the non-excluded line count).
