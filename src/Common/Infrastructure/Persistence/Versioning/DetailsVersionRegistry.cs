using System.Text.Json;

namespace Rtl.Core.Infrastructure.Persistence.Versioning;

public sealed class DetailsVersionRegistry<T>(int currentVersion) where T : class
{
    private readonly int _currentVersion = currentVersion;
    private readonly SortedList<int, IDetailsUpgrader<T>> _upgraders = [];

    public DetailsVersionRegistry<T> Register(IDetailsUpgrader<T> upgrader)
    {
        if (upgrader.ToVersion != upgrader.FromVersion + 1)
        {
            throw new ArgumentException(
                $"Upgrader must be sequential: expected FromVersion {upgrader.FromVersion} → ToVersion {upgrader.FromVersion + 1}, " +
                $"got {upgrader.FromVersion} → {upgrader.ToVersion}.");
        }

        if (_upgraders.ContainsKey(upgrader.FromVersion))
        {
            throw new ArgumentException(
                $"An upgrader from version {upgrader.FromVersion} is already registered.");
        }

        _upgraders.Add(upgrader.FromVersion, upgrader);
        return this;
    }

    public JsonDocument UpgradeToCurrentVersion(JsonDocument doc, int fromVersion)
    {
        if (fromVersion >= _currentVersion)
            return doc;

        var current = doc;

        for (var v = fromVersion; v < _currentVersion; v++)
        {
            if (!_upgraders.TryGetValue(v, out var upgrader))
            {
                throw new InvalidOperationException(
                    $"No upgrader registered for {typeof(T).Name} from version {v} to {v + 1}.");
            }

            var upgraded = upgrader.Upgrade(current);

            if (!ReferenceEquals(current, doc))
                current.Dispose();

            current = upgraded;
        }

        return current;
    }
}
