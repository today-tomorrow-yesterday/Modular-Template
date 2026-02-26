# SalesTeamLine — Pricing Guide

**In GP?** No — purely metadata. All three pricing fields are always zero.

---

## What the user sends

```json
PUT /packages/{id}/sales-team
{
    "members": [
        { "authorizedUserId": 42, "role": "Primary", "commissionSplitPercentage": 60 },
        { "authorizedUserId": 87, "role": "Secondary", "commissionSplitPercentage": 40 }
    ]
}
```

**No pricing fields in the request.** The user sends team composition and commission
split percentages — nothing about dollar amounts.

## What happens to the three pricing fields

```
(no user input for pricing)      →  SalesTeamLine.SalePrice = 0         (always)
                                 →  SalesTeamLine.EstimatedCost = 0     (always)
                                 →  SalesTeamLine.RetailSalePrice = 0   (always)
```

All three are hardcoded to zero at creation. No user input can change them.

---

## GP impact

```
GP += 0   (excluded, and all zeros anyway)
```

---

## What this line actually stores

The value is in `SalesTeamDetails`, not the pricing fields:

- **Members** — each with AuthorizedUserId, EmployeeNumber (resolved from cache),
  DisplayName, Role (Primary/Secondary), and CommissionSplitPercentage
- Split percentages must sum to exactly 100

These are consumed later by `CalculateCommission` when it builds the commission
request for iSeries.

---

## No side effects

No tax change. No domain events. No auto-generated project costs.

---

## What the API returns

```json
{ "grossProfit": 17000, "commissionableGrossProfit": 0, "mustRecalculateTaxes": false }
```

Returns the standard triple for API consistency, but this handler never changes
any of them.
