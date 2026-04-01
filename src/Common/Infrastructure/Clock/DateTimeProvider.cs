using ModularTemplate.Domain;

namespace ModularTemplate.Infrastructure.Clock;

/// <summary>
/// Default implementation of IDateTimeProvider.
/// </summary>
internal sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
