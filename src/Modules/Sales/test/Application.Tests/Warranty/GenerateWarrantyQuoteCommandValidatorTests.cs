using FluentValidation.TestHelper;
using Modules.Sales.Application.Insurance.GenerateWarrantyQuote;
using Xunit;

namespace Modules.Sales.Application.Tests.Warranty;

public sealed class GenerateWarrantyQuoteCommandValidatorTests
{
    private readonly GenerateWarrantyQuoteCommandValidator _sut = new();

    [Fact]
    public void Valid_command_passes_validation()
    {
        var command = new GenerateWarrantyQuoteCommand(Guid.NewGuid());

        var result = _sut.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_sale_public_id_fails_validation()
    {
        var command = new GenerateWarrantyQuoteCommand(Guid.Empty);

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.SalePublicId);
    }
}
