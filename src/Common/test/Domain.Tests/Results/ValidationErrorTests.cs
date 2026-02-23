using Rtl.Core.Domain.Results;
using Xunit;

namespace Rtl.Core.Domain.Tests.Results;

public class ValidationErrorTests
{
    [Fact]
    public void Constructor_SetsErrorsAndBaseProperties()
    {
        var errors = new[] { Error.Validation("Field1", "Error 1") };

        var validationError = new ValidationError(errors);

        Assert.Single(validationError.Errors);
        Assert.Equal("General.Validation", validationError.Code);
        Assert.Equal(ErrorType.Validation, validationError.Type);
    }

    [Fact]
    public void FromResults_ExtractsErrorsFromFailedResults()
    {
        var results = new[]
        {
            Result.Failure(Error.Validation("Field1", "Error 1")),
            Result.Success(),
            Result.Failure(Error.Validation("Field2", "Error 2"))
        };

        var validationError = ValidationError.FromResults(results);

        Assert.Equal(2, validationError.Errors.Length);
    }
}
