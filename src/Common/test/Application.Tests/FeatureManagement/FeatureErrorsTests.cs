using Rtl.Core.Application.FeatureManagement;
using Rtl.Core.Domain.Results;
using Xunit;

namespace Rtl.Core.Application.Tests.FeatureManagement;

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
