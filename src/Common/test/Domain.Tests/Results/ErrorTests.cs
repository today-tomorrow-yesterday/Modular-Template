using ModularTemplate.Domain.Results;
using Xunit;

namespace ModularTemplate.Domain.Tests.Results;

public class ErrorTests
{
    [Fact]
    public void FactoryMethods_CreateCorrectErrorTypes()
    {
        Assert.Equal(ErrorType.Failure, Error.Failure("code", "desc").Type);
        Assert.Equal(ErrorType.Validation, Error.Validation("code", "desc").Type);
        Assert.Equal(ErrorType.NotFound, Error.NotFound("code", "desc").Type);
        Assert.Equal(ErrorType.Conflict, Error.Conflict("code", "desc").Type);
    }

    [Fact]
    public void PredefinedErrors_HaveExpectedValues()
    {
        Assert.Equal(string.Empty, Error.None.Code);
        Assert.Equal("General.Null", Error.NullValue.Code);
    }
}
