using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Rtl.Core.Domain.Results;
using Rtl.Core.Presentation.Results;
using Xunit;

namespace Rtl.Core.Presentation.Tests.Results;

#pragma warning disable CS0618 // Testing the obsolete class intentionally
public class ApiResultsTests
{
    [Fact]
    public void Problem_WithSuccessResult_ThrowsInvalidOperationException()
    {
        var result = Result.Success();

        Assert.Throws<InvalidOperationException>(() => ApiResults.Problem(result));
    }

    [Fact]
    public void Problem_WithNotFoundError_Returns404()
    {
        var error = Error.NotFound("Entity.NotFound", "Not found");

        var result = ApiResults.Problem(error) as ProblemHttpResult;

        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
    }

    [Fact]
    public void Problem_WithValidationError_Returns400()
    {
        var error = Error.Validation("Field.Invalid", "Invalid");

        var result = ApiResults.Problem(error) as ProblemHttpResult;

        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
    }

    [Fact]
    public void Problem_WithFailureError_Returns500()
    {
        var error = Error.Failure("Server.Error", "Error");

        var result = ApiResults.Problem(error) as ProblemHttpResult;

        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
    }
}
