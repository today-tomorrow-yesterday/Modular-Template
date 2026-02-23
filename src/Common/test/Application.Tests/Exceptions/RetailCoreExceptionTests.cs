using Rtl.Core.Application.Exceptions;
using Rtl.Core.Domain.Results;
using Xunit;

namespace Rtl.Core.Application.Tests.Exceptions;

public class RetailCoreExceptionTests
{
    [Fact]
    public void Constructor_WithRequestNameOnly_SetsMessage()
    {
        var exception = new RetailCoreException("TestRequest");

        Assert.Equal("Exception processing request 'TestRequest'", exception.Message);
        Assert.Equal("TestRequest", exception.RequestName);
    }

    [Fact]
    public void Constructor_WithError_IncludesErrorInMessage()
    {
        var error = Error.Failure("Test.Error", "Test description");

        var exception = new RetailCoreException("TestRequest", error);

        Assert.Contains("[Test.Error]", exception.Message);
        Assert.Contains("Test description", exception.Message);
        Assert.Equal(error, exception.Error);
    }

    [Fact]
    public void Constructor_WithInnerException_IncludesInnerExceptionMessage()
    {
        var innerException = new InvalidOperationException("Inner error message");

        var exception = new RetailCoreException("TestRequest", innerException: innerException);

        Assert.Contains("Inner error message", exception.Message);
        Assert.Equal(innerException, exception.InnerException);
    }
}
