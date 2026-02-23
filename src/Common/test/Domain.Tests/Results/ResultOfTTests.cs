using Rtl.Core.Domain.Results;
using Xunit;

namespace Rtl.Core.Domain.Tests.Results;

public class ResultOfTTests
{
    [Fact]
    public void Value_ReturnsValueForSuccess()
    {
        var result = Result.Success(42);

        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Value_ThrowsForFailure()
    {
        var result = Result.Failure<int>(Error.Failure("Test", "Test"));

        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void ImplicitConversion_FromNullCreatesFailure()
    {
        Result<string> result = (string)null!;

        Assert.True(result.IsFailure);
        Assert.Equal(Error.NullValue, result.Error);
    }
}
