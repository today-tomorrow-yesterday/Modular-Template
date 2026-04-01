using System.Text.Json;

namespace ModularTemplate.Domain.Entities;

public interface IVersionedDetails
{
    int SchemaVersion { get; }
    Dictionary<string, JsonElement>? ExtensionData { get; set; }
}
