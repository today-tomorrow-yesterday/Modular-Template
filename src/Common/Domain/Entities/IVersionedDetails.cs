using System.Text.Json;

namespace Rtl.Core.Domain.Entities;

public interface IVersionedDetails
{
    int SchemaVersion { get; }
    Dictionary<string, JsonElement>? ExtensionData { get; set; }
}
