using ModularTemplate.Application.FeatureManagement;
using ModularTemplate.Domain.Results;
using Xunit;

namespace ModularTemplate.Application.Tests.FeatureManagement;

public class FeatureErrorsTests
{
    [Fact]
    public void FeatureDisabled_ReturnsCorrectError()
    {
        var error = FeatureErrors.FeatureDisabled("MyFeature");

        Assert.Equal("Feature.Disabled", error.Code);
        Assert.Contains("MyFeature", error.Description);
        Assert.Equal(ErrorType.Failure, error.Type);
    }
}
