using System.Text.Json;
using System.Text.Json.Serialization;
using ModularTemplate.Domain.Entities;

namespace Modules.SampleOrders.Domain.Orders;

/// <summary>
/// JSONB details for a custom/ad-hoc order line — miscellaneous charges, adjustments, etc.
/// Implements IVersionedDetails for forward-compatible schema evolution.
/// </summary>
public sealed class CustomLineDetails : IVersionedDetails
{
    public int SchemaVersion { get; set; } = 1;

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }

    public string? Description { get; set; }
    public string? Reason { get; set; }
    public string? ApprovedBy { get; set; }
}
