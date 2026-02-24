using Modules.Sales.Application.Tax.GetTaxQuestions;
using Modules.Sales.Domain.Cdc;
using NSubstitute;
using Xunit;

namespace Modules.Sales.Application.Tests.Tax;

public sealed class GetTaxQuestionsQueryHandlerTests
{
    private readonly ICdcTaxQueries _cdcTaxQueries = Substitute.For<ICdcTaxQueries>();
    private readonly GetTaxQuestionsQueryHandler _sut;

    public GetTaxQuestionsQueryHandlerTests() =>
        _sut = new GetTaxQuestionsQueryHandler(_cdcTaxQueries);

    [Fact]
    public async Task Returns_success_with_empty_list_when_no_questions()
    {
        _cdcTaxQueries.GetQuestionsForStateAndHomeTypeAsync("OH", 1, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<TaxQuestionWithText>());

        var result = await _sut.Handle(
            new GetTaxQuestionsQuery("OH", 1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task Returns_success_with_mapped_questions()
    {
        var question = new TaxQuestionWithText(1, "Is the home on leased land?");

        _cdcTaxQueries.GetQuestionsForStateAndHomeTypeAsync("TX", 2, Arg.Any<CancellationToken>())
            .Returns(new List<TaxQuestionWithText> { question });

        var result = await _sut.Handle(
            new GetTaxQuestionsQuery("TX", 2), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal(1, result.Value.First().QuestionNumber);
        Assert.Equal("Is the home on leased land?", result.Value.First().Text);
    }
}
