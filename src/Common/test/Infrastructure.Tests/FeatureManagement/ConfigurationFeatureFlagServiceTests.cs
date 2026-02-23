using Microsoft.Extensions.Configuration;
using Rtl.Core.Infrastructure.FeatureManagement;
using Xunit;

namespace Rtl.Core.Infrastructure.Tests.FeatureManagement;

public class ConfigurationFeatureFlagServiceTests
{
    [Fact]
    public void IsEnabled_WhenFeatureIsTrue_ReturnsTrue()
    {
        var configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            { "Features:TestFeature", "true" }
        });
        var service = new ConfigurationFeatureFlagService(configuration);

        Assert.True(service.IsEnabled("Features:TestFeature"));
    }

    [Fact]
    public void IsEnabled_WhenFeatureIsFalse_ReturnsFalse()
    {
        var configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            { "Features:TestFeature", "false" }
        });
        var service = new ConfigurationFeatureFlagService(configuration);

        Assert.False(service.IsEnabled("Features:TestFeature"));
    }

    [Fact]
    public void IsEnabled_WhenFeatureMissing_ReturnsFalse()
    {
        var configuration = CreateConfiguration(new Dictionary<string, string?>());
        var service = new ConfigurationFeatureFlagService(configuration);

        Assert.False(service.IsEnabled("Features:NonExistentFeature"));
    }

    private static IConfiguration CreateConfiguration(Dictionary<string, string?> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}
