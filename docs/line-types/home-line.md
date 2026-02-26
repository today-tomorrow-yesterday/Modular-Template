# HomeLine — Pricing Guide

**In GP?** Yes — `GP += SalePrice - EstimatedCost`

---

## What the user sends

```
PUT /packages/{id}/home
{
    "salePrice": 90000,           ← negotiated price to the customer
    "estimatedCost": 70000,       ← dealer's invoice/cost for the home
    "retailSalePrice": 100000,    ← sticker price / MSRP ceiling
    "stockNumber": "ABC123",
    "homeType": "New",
    ... (40+ other fields: specs, costs, transport, classification)
}
```

## What happens to the three pricing fields

```
User sends salePrice: 90000        →  HomeLine.SalePrice = 90000          no transformation
User sends estimatedCost: 70000    →  HomeLine.EstimatedCost = 70000      no transformation
User sends retailSalePrice: 100000 →  HomeLine.RetailSalePrice = 100000   no transformation
```

**All three pass straight through.** The handler rounds to 2 decimal places
(`Math.Round(value, 2)`) but does not compute, derive, or overwrite any of them.
What the user sends is what gets stored.

None of the other 40+ fields in the request (StockNumber, HomeType, BaseCost,
FreightCost, etc.) affect these three pricing fields. Those are stored in
`HomeDetails` (JSONB) and are used later by iSeries calls (tax, commission,
allowances) — but they never modify SalePrice, EstimatedCost, or RetailSalePrice
on the HomeLine itself.

---

## GP impact

```
GP += 90000 - 70000 = 20000
```

HomeLine is typically the largest contributor to Gross Profit.

---

## Side effect: W&A project cost (separate line, not the HomeLine)

If the user provides `wheelAndAxlesOption` (Rent or Purchase) along with either
a stock number or wheel/axle counts, the handler calls iSeries to price wheels
and axles. This creates a **separate** ProjectCostLine — it does NOT touch the
HomeLine's pricing fields.

```
User sends wheelAndAxlesOption: "Rent"
  + stockNumber: "ABC123"               → iSeries returns { SalePrice: 1000, Cost: 500 }
  OR numberOfWheels: 4, numberOfAxles: 2 → iSeries returns { SalePrice: 1000, Cost: 500 }

  → creates ProjectCostLine (Cat 1, Item 28)
      SalePrice = 1000, EstimatedCost = 500
      GP += 1000 - 500 = 500
```

---

## What the API returns

```json
{ "grossProfit": 20500, "commissionableGrossProfit": 0, "mustRecalculateTaxes": true }
```

No individual line pricing echoed back.
