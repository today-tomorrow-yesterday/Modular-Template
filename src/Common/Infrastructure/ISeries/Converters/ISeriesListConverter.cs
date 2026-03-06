using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rtl.Core.Infrastructure.ISeries.Converters;

/// <summary>
/// Handles iSeries responses that wrap lists in Newtonsoft.Json TypeNameHandling format:
/// <code>{ "$type": "System.Collections.Generic.List`1[...]", "$values": ["item1", "item2"] }</code>
/// Extracts the "$values" array and deserializes it as a standard List&lt;T&gt;.
/// Returns an empty list if "$values" is not found.
/// </summary>
internal sealed class ISeriesListConverter<T> : JsonConverter<List<T>>
{
    public override List<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);

        if (!doc.RootElement.TryGetProperty("$values", out var values))
            return [];

        return [.. values.EnumerateArray().Select(item => item.Deserialize<T>(options)!)];
    }

    public override void Write(Utf8JsonWriter writer, List<T> value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, value, options);
}
