using System.Text.Json;

namespace ModularTemplate.Infrastructure.Persistence.Versioning;

public interface IDetailsUpgrader<T> where T : class
{
    int FromVersion { get; }
    int ToVersion { get; }
    JsonDocument Upgrade(JsonDocument document);
}
