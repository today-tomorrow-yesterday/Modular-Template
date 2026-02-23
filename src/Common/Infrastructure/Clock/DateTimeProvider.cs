using Rtl.Core.Domain;

namespace Rtl.Core.Infrastructure.Clock;

/// <summary>
/// Default implementation of IDateTimeProvider.
/// </summary>
internal sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
