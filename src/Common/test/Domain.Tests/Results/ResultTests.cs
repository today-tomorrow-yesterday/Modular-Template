using Rtl.Core.Domain.Results;
using Xunit;

namespace Rtl.Core.Domain.Tests.Results;

public class ResultTests
{
    [Fact]
    public void Success_CreatesSuccessfulResult()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
    }

    [Fact]
    public void Failure_CreatesFailedResult()
    {
        var error = Error.Failure("Test.Error", "Test error");

        var result = Result.Failure(error);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Match_ExecutesCorrectBranch()
    {
        var success = Result.Success();
        var failure = Result.Failure(Error.Failure("Test", "Test"));

        Assert.Equal("ok", success.Match(() => "ok", _ => "fail"));
        Assert.Equal("fail", failure.Match(() => "ok", _ => "fail"));
    }
}
