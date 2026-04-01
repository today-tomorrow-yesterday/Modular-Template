using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ModularTemplate.Domain.Entities;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace ModularTemplate.Infrastructure.Persistence.Versioning;

public sealed class VersionedJsonConverter<T> : ValueConverter<T?, string>
    where T : class, IVersionedDetails
{
    private const string VersionKey = "_v";
    private const string SchemaVersionPropertyName = "schemaVersion";

    private static readonly JsonSerializerOptions SerializerOptions = CreateSerializerOptions();

    public VersionedJsonConverter(DetailsVersionRegistry<T>? registry = null)
        : base(
            v => v == null ? "" : Serialize(v),
            v => string.IsNullOrEmpty(v) ? null : Deserialize(v, registry))
    {
    }

    private static JsonSerializerOptions CreateSerializerOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() },
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers = { EnablePrivateMembers }
            }
        };

        return options;
    }

    private static void EnablePrivateMembers(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object)
            return;

        foreach (var prop in typeInfo.Properties)
        {
            if (prop.Set is not null)
                continue;

            // Find the CLR property by camelCase name to wire up private setters
            var clrProperty = FindPropertyByCamelCase(typeInfo.Type, prop.Name);

            if (clrProperty?.SetMethod is not null)
            {
                prop.Set = (obj, value) => clrProperty.SetMethod.Invoke(obj, [value]);
            }
        }

        // Mark ExtensionData as extension data (captures unknown JSON properties)
        foreach (var prop in typeInfo.Properties)
        {
            if (prop.Name == "extensionData" && prop.PropertyType == typeof(Dictionary<string, JsonElement>))
            {
                prop.IsExtensionData = true;
                break;
            }
        }

        // Enable private parameterless constructor
        if (typeInfo.CreateObject is null)
        {
            var privateConstructor = typeInfo.Type.GetConstructor(
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                binder: null,
                types: Type.EmptyTypes,
                modifiers: null);

            if (privateConstructor is not null)
            {
                typeInfo.CreateObject = () => privateConstructor.Invoke(null);
            }
        }
    }

    private static System.Reflection.PropertyInfo? FindPropertyByCamelCase(Type type, string camelCaseName)
    {
        var properties = type.GetProperties(
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);

        foreach (var prop in properties)
        {
            if (JsonNamingPolicy.CamelCase.ConvertName(prop.Name) == camelCaseName)
                return prop;
        }

        return null;
    }

    private static string Serialize(T value)
    {
        var json = JsonSerializer.Serialize(value, SerializerOptions);
        using var doc = JsonDocument.Parse(json);
        using var stream = new System.IO.MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();

            // Write _v first
            writer.WriteNumber(VersionKey, value.SchemaVersion);

            // Write all other properties, skipping schemaVersion
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (prop.Name.Equals(SchemaVersionPropertyName, StringComparison.OrdinalIgnoreCase))
                    continue;

                prop.WriteTo(writer);
            }

            writer.WriteEndObject();
        }

        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }

    private static T Deserialize(string json, DetailsVersionRegistry<T>? registry)
    {
        var doc = JsonDocument.Parse(json);
        var version = doc.RootElement.TryGetProperty(VersionKey, out var vProp) ? vProp.GetInt32() : 1;

        if (registry is not null)
        {
            var upgraded = registry.UpgradeToCurrentVersion(doc, version);
            if (!ReferenceEquals(upgraded, doc))
                doc.Dispose();
            doc = upgraded;
        }

        try
        {
            var cleanJson = StripVersionKey(doc);
            return JsonSerializer.Deserialize<T>(cleanJson, SerializerOptions)
                ?? throw new JsonException($"Failed to deserialize {typeof(T).Name} from JSONB.");
        }
        finally
        {
            doc.Dispose();
        }
    }

    private static string StripVersionKey(JsonDocument doc)
    {
        using var stream = new System.IO.MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (prop.Name == VersionKey)
                    continue;

                prop.WriteTo(writer);
            }
            writer.WriteEndObject();
        }

        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }
}
