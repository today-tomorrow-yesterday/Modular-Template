# TaxLine — Pricing Guide

**In GP?** Yes — `GP += SalePrice - 0` = the total tax amount

---

## Two operations, different pricing behavior

### Operation 1: Configure tax settings (`PUT /packages/{id}/tax`)

```
PUT /packages/{id}/tax
{
    "previouslyTitled": "Yes",
    "taxExemptionId": null,
    "questionAnswers": [{ "questionNumber": 1, "answer": true }]
}
```

**No pricing fields in the request.** The user only sends configuration — nothing
about dollar amounts. All three pricing fields are hardcoded to zero:

```
No user input for pricing     →  TaxLine.SalePrice = 0
                              →  TaxLine.EstimatedCost = 0
                              →  TaxLine.RetailSalePrice = 0
                              →  MustRecalculateTaxes = true  (always)
```

This endpoint saves tax config and clears any previous calculations.

---

### Operation 2: Calculate taxes (`POST /packages/{id}/tax`)

```
POST /packages/{id}/tax
(empty body — no user input)
```

**No user input at all.** The handler reads existing tax config from the TaxLine,
gathers pricing data from all other lines on the package, and sends it to iSeries.

The three pricing fields are set entirely from iSeries results:

```
iSeries returns:
  StateTax: 1000
  CityTax: 500
  CountyTax: 250
  GrossReceiptCityTax: null    (TN only)
  GrossReceiptCountyTax: null  (TN only)
  MHIT: null                  (TX only)
  UseTax: 250

→  TaxLine.SalePrice     = 1000 + 500 + 250 + 0 + 0 + 0 = 1750   (sum of tax components)
→  TaxLine.EstimatedCost = 0                                       (always zero)
→  TaxLine.RetailSalePrice = 0                                     (always zero)
```

**No user-provided value drives SalePrice.** It's computed entirely from the iSeries
tax calculation, which itself is based on the HomeLine's sale price, project costs,
trade-in data, delivery address, and tax configuration.

---

## GP impact

```
GP += 1750 - 0 = 1750
```

The total tax amount enters GP as pure revenue (no cost offset).

---

## Side effect: Use Tax project cost

When iSeries returns `UseTax > 0`, a separate ProjectCostLine is created:

```
iSeries returns UseTax: 250

→  ProjectCostLine (Cat 9, Item 21)
     SalePrice = 250, EstimatedCost = 250
     GP += 250 - 250 = 0   (washes out — same value for both)
```

---

## What the API returns

**Configure:** `{ "grossProfit": ..., "mustRecalculateTaxes": true }`

**Calculate:**
```json
{
    "grossProfit": 21750,
    "mustRecalculateTaxes": false,
    "taxSalePrice": 1750,
    "taxItems": [
        { "name": "State Tax", "calculatedAmount": 1000, "chargedAmount": 1000 },
        { "name": "City Tax", "calculatedAmount": 500, "chargedAmount": 500 },
        ...
    ],
    "errors": []
}
```

This is the only endpoint that returns a per-component breakdown.
