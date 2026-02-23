using System.Text.Json;

namespace Rtl.Core.Infrastructure.Persistence.Versioning;

public interface IDetailsUpgrader<T> where T : class
{
    int FromVersion { get; }
    int ToVersion { get; }
    JsonDocument Upgrade(JsonDocument document);
}
