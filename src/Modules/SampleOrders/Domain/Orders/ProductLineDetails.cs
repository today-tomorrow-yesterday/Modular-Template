using System.Text.Json;
using System.Text.Json.Serialization;
using ModularTemplate.Domain.Entities;

namespace Modules.SampleOrders.Domain.Orders;

/// <summary>
/// JSONB details for a product order line — snapshot of product data at time of order.
/// Implements IVersionedDetails for forward-compatible schema evolution.
/// </summary>
public sealed class ProductLineDetails : IVersionedDetails
{
    public int SchemaVersion { get; set; } = 1;

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }

    public string? ProductName { get; set; }
    public string? Sku { get; set; }
    public string? Category { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public decimal? Length { get; set; }
    public string? Notes { get; set; }
}
