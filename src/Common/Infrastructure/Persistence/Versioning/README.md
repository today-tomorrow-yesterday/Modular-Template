# JSONB Details Versioning

JSONB `Details` columns use a versioned schema managed by three components:

| Component | Role |
|-----------|------|
| `VersionedJsonConverter<T>` | EF Core `ValueConverter` — serializes/deserializes `T` with version stamping |
| `DetailsVersionRegistry<T>` | Holds an ordered chain of upgraders for a given `T` |
| `IDetailsUpgrader<T>` | Single-step upgrader: transforms JSON from version N to N+1 |

## How It Works

### Storage format

Every Details column is stored as a `jsonb` value with a `_v` key:

```json
{"_v": 1, "splitPercent": 50, "managerName": "Jones"}
```

`_v` is stripped before deserialization and re-added on write — C# classes never see it.

### Read path

1. Parse JSON, read `_v` (defaults to 1 if absent)
2. If a `DetailsVersionRegistry<T>` is registered, run the upgrade chain: v1 → v2 → ... → current
3. Strip `_v`, deserialize into `T`
4. Unknown JSON properties are captured into `ExtensionData` (see below)

### Write path

1. Serialize `T` to JSON (includes `ExtensionData` overflow as top-level properties)
2. Rebuild with `_v` as the first key, skip `schemaVersion`
3. Write to column

## Contract: `IVersionedDetails`

Every Details class implements this interface:

```csharp
public interface IVersionedDetails
{
    int SchemaVersion { get; }
    Dictionary<string, JsonElement>? ExtensionData { get; set; }
}
```

- `SchemaVersion` — the version this C# class represents (e.g. `=> 2`). Written as `_v` in JSON.
- `ExtensionData` — captures any JSON properties that don't map to a C# property. Written back as top-level properties on save.

## Extension Data Preservation

When a schema evolves, properties may be added, renamed, or removed. Without `ExtensionData`, re-saving a row through a newer model silently discards properties the model doesn't know about.

With `ExtensionData`, unknown properties survive read/write cycles:

```
DB row (v1):  {"_v":1, "a":1, "b":2, "splitPercent":50}

Read on v2 model (which only has SplitPercent):
  C#:       SplitPercent = 50
            ExtensionData = {"a": 1, "b": 2}

Re-saved:   {"_v":2, "splitPercent":50, "a":1, "b":2}
```

Properties `a` and `b` are invisible to application code but preserved in the JSON for audit and rollback purposes.

### How it's wired

`ExtensionData` is marked as `IsExtensionData = true` by convention in the `VersionedJsonConverter` type info modifier — no `[JsonExtensionData]` attribute needed on domain classes. The convention matches by property name (`extensionData` in camelCase) and type (`Dictionary<string, JsonElement>`).

## Writing an Upgrader

Implement `IDetailsUpgrader<T>` for each version step:

```csharp
public sealed class HomeDetailsV1ToV2 : IDetailsUpgrader<HomeDetails>
{
    public int FromVersion => 1;
    public int ToVersion => 2;

    public JsonDocument Upgrade(JsonDocument document)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            foreach (var prop in document.RootElement.EnumerateObject())
            {
                prop.WriteTo(writer);
            }
            // Add new property with default value
            writer.WriteNumber("newField", 0);
            writer.WriteEndObject();
        }
        return JsonDocument.Parse(stream.ToArray());
    }
}
```

Register it in the `DetailsVersionRegistry<T>`:

```csharp
var registry = new DetailsVersionRegistry<HomeDetails>(currentVersion: 2)
    .Register(new HomeDetailsV1ToV2());
```

### Upgrader rules

| Schema Change | What the Upgrader Does | What Happens to Old Data |
|---------------|------------------------|--------------------------|
| **Add property** | Add it to JSON with a sensible default | N/A |
| **Rename property** | Copy value to new name; leave old name in place | Old name becomes `ExtensionData` |
| **Change type** | Add new-typed property; leave old property | Old property becomes `ExtensionData` |
| **Remove property** | Do nothing | Property automatically becomes `ExtensionData` |

**Never delete properties in an upgrader.** Orphaned properties are captured by `ExtensionData` and survive re-saves. This ensures zero data loss across schema versions.

### Version numbering

- Versions are sequential integers starting at 1.
- Each upgrader goes from exactly N to N+1 (enforced by `DetailsVersionRegistry`).
- Bump `SchemaVersion` on the C# class to match the latest version.
- The registry's `currentVersion` must equal the C# class's `SchemaVersion`.

### Checklist for a schema change

1. Write the `IDetailsUpgrader<T>` implementation (N → N+1)
2. Update the C# Details class (add/rename properties, bump `SchemaVersion`)
3. Register the upgrader in the `DetailsVersionRegistry<T>`
4. Regenerate the EF migration (column schema won't change — it's still `jsonb`)
5. Verify `dotnet build` passes with 0 errors
