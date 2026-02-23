namespace Modules.SampleSales.Application.FeatureManagement;

/// <summary>
/// Configuration paths for SampleSales module feature flags.
/// These flags are defined in modules.samplesales.json under SampleSales:Features.
/// </summary>
public static class SampleSalesFeatures
{
    /// <summary>
    /// Configuration path for the Catalog V2 Pagination feature flag.
    /// Controls whether the V2 paginated catalog endpoint is available.
    /// </summary>
    public const string CatalogV2Pagination = "SampleSales:Features:CatalogV2Pagination";

    /// <summary>
    /// Configuration path for the Bulk Create Products feature flag.
    /// Controls whether bulk product creation operations are enabled.
    /// </summary>
    public const string BulkCreateProducts = "SampleSales:Features:BulkCreateProducts";
}
