using ModularTemplate.Infrastructure.Clock;
using Xunit;

namespace ModularTemplate.Infrastructure.Tests.Clock;

public class DateTimeProviderTests
{
    [Fact]
    public void UtcNow_ReturnsCurrentUtcTime()
    {
        var provider = new DateTimeProvider();
        var before = DateTime.UtcNow;

        var result = provider.UtcNow;

        var after = DateTime.UtcNow;
        Assert.True(result >= before && result <= after);
    }

    [Fact]
    public void UtcNow_ReturnsUtcKind()
    {
        var provider = new DateTimeProvider();

        var result = provider.UtcNow;

        Assert.Equal(DateTimeKind.Utc, result.Kind);
    }
}
