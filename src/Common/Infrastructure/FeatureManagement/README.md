# Feature Management

Configuration-based feature toggles using `IConfiguration`.

## Two Tiers

| Scope | Config Location | Example Path |
|-------|-----------------|--------------|
| Infrastructure | `appsettings.json` under `Features:Infrastructure` | `Features:Infrastructure:Outbox` |
| Module | `appsettings.json` under `{ModuleName}:Features` | `SampleSales:Features:CatalogV2Pagination` |

> **Note**: Module-specific config files (`modules.{module}.json`) do not currently exist. All configuration is in the main `appsettings.json` file.

## Infrastructure Features

Global flags in `appsettings.json`:

```json
{
  "Features": {
    "Infrastructure": {
      "Outbox": true,
      "Inbox": true,
      "BackgroundJobs": true,
      "Emails": true,
      "CdcEvents": true
    }
  }
}
```

| Flag | Controls |
|------|----------|
| `Outbox` | Domain event publishing via outbox |
| `Inbox` | Integration event consumption |
| `BackgroundJobs` | Quartz job execution |
| `Emails` | Email sending |
| `CdcEvents` | Change Data Capture events |

**Defined in**: `InfrastructureFeatures.cs`

## Module Features

Module flags are configured in `appsettings.json` under `{ModuleName}:Features`:

```json
{
  "SampleSales": {
    "Features": {
      "CatalogV2Pagination": true,
      "BulkCreateProducts": false
    }
  }
}
```

**Naming Convention**: Module feature paths use the actual module name as the prefix (e.g., `SampleSales:`, `SampleOrders:`, `Customer:`), not abbreviated or generic names.

**Define in module**:
```csharp
public static class SampleSalesFeatures
{
    public const string CatalogV2Pagination = "SampleSales:Features:CatalogV2Pagination";
    public const string BulkCreateProducts = "SampleSales:Features:BulkCreateProducts";
}
```

## Usage

### In Endpoints (returns 404 when disabled)
```csharp
group.MapGet("/v2/products", GetAllV2Async)
    .RequireFeature(SampleSalesFeatures.CatalogV2Pagination);
```

### In Handlers
```csharp
public class CreateProductHandler(IFeatureFlagService features)
{
    public async Task<Result<Guid>> Handle(...)
    {
        if (!await features.IsEnabledAsync(SampleSalesFeatures.BulkCreateProducts))
            return Result.Failure(FeatureErrors.FeatureDisabled("BulkCreateProducts"));

        // Feature logic
    }
}
```

### In Background Jobs
```csharp
if (!_featureFlags.IsEnabled(InfrastructureFeatures.Outbox))
    return; // Skip processing
```

## Implementation

`ConfigurationFeatureFlagService` reads from `IConfiguration`:
- Path notation: `Section:Subsection:Flag`
- Missing flags default to `false` (disabled)
- Boolean parsing: `true`/`false` strings

## Microservice Extraction

Module flags are namespaced under the module name in configuration. When extracting a module to a microservice, its feature flags can be moved to the microservice's own `appsettings.json` without path changes.
